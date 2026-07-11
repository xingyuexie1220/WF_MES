using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Refit;
using Serilog;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Api;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services;

public sealed class ApiAuthService(
    IAuthApi authApi,
    IApiTokenStore tokenStore,
    ISessionService sessionService,
    ILocalizationService localization,
    IConfiguration configuration) : IAuthService
{
    private static readonly TimeSpan RefreshBeforeExpiry = TimeSpan.FromMinutes(2);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly string? _apiBaseUrl = configuration["Api:BaseUrl"];

    public async Task<LoginResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        tokenStore.Clear();

        try
        {
            var response = await authApi.LoginAsync(new LoginRequestDto
            {
                UserName = userName.Trim(),
                Password = password,
                ClientType = 3,
                Locale = localization.CurrentLocale
            }, cancellationToken);

            if (response.Code != 200 || response.Data is null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                        ? localization.T("auth.invalid_credentials", "账号或密码错误")
                        : response.Message
                };
            }

            if (response.Data.NeedSelectFactory)
            {
                return new LoginResultDto
                {
                    Success = false,
                    NeedSelectFactory = true,
                    Factories = response.Data.Factories
                };
            }

            ApplyLoginResponse(response.Data);
            return new LoginResultDto
            {
                Success = true,
                User = response.Data.UserInfo,
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken
            };
        }
        catch (Exception ex)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = ApiErrorHelper.ToUserMessage(ex, _apiBaseUrl)
            };
        }
    }

    public async Task<LoginResultDto> SelectFactoryAsync(string userName, string password, long factoryId, CancellationToken cancellationToken = default)
    {
        tokenStore.Clear();

        try
        {
            var response = await authApi.SelectFactoryAsync(new SelectFactoryRequestDto
            {
                UserName = userName.Trim(),
                Password = password,
                FactoryId = factoryId,
                ClientType = 3,
                Locale = localization.CurrentLocale
            }, cancellationToken);

            if (response.Code != 200 || response.Data is null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                        ? localization.T("desktop.factory.selectFailed", "选厂失败")
                        : response.Message
                };
            }

            ApplyLoginResponse(response.Data);
            return new LoginResultDto
            {
                Success = true,
                User = response.Data.UserInfo,
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken
            };
        }
        catch (Exception ex)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = ApiErrorHelper.ToUserMessage(ex, _apiBaseUrl)
            };
        }
    }

    public async Task<LoginResultDto> SwitchFactoryAsync(long factoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await authApi.SwitchFactoryAsync(new SwitchFactoryRequestDto
            {
                FactoryId = factoryId
            }, cancellationToken);

            if (response.Code != 200 || response.Data is null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                        ? localization.T("desktop.factory.switchFailed", "切换工厂失败")
                        : response.Message
                };
            }

            ApplyLoginResponse(response.Data);
            return new LoginResultDto
            {
                Success = true,
                User = response.Data.UserInfo,
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken
            };
        }
        catch (Exception ex)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessage = ApiErrorHelper.ToUserMessage(ex, _apiBaseUrl)
            };
        }
    }

    public async Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenStore.AccessToken))
        {
            return false;
        }

        if (ShouldRefreshProactively())
        {
            await TryRefreshTokenAsync(cancellationToken);
        }

        if (await PingSessionAsync(cancellationToken))
        {
            return true;
        }

        if (!await TryRefreshTokenAsync(cancellationToken))
        {
            return false;
        }

        return await PingSessionAsync(cancellationToken);
    }

    public Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken = default)
        => RefreshTokensInternalAsync(cancellationToken);

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(tokenStore.AccessToken))
            {
                await authApi.LogoutAsync(cancellationToken);
            }
        }
        catch
        {
            // 退出时忽略网络错误
        }
        finally
        {
            tokenStore.Clear();
        }
    }

    public async Task<UserInfoDto?> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var response = await authApi.ChangePasswordAsync(new ChangePasswordRequestDto
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        }, cancellationToken);

        var user = ApiResponseHelper.EnsureData(response, "修改密码失败");
        sessionService.SetUser(user);
        sessionService.SetActionPermissions(user.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
        return user;
    }

    private bool ShouldRefreshProactively()
    {
        if (string.IsNullOrWhiteSpace(tokenStore.RefreshToken))
        {
            return false;
        }

        var expiresAt = tokenStore.AccessTokenExpiresAt;
        return expiresAt is null || expiresAt <= DateTimeOffset.UtcNow.Add(RefreshBeforeExpiry);
    }

    private async Task<bool> PingSessionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await authApi.GetInfoAsync(cancellationToken);
            if (response.Code != 200 || response.Data is null)
            {
                return false;
            }

            sessionService.SetUser(response.Data);
            sessionService.SetActionPermissions(response.Data.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> RefreshTokensInternalAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenStore.RefreshToken))
        {
            return false;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (string.IsNullOrWhiteSpace(tokenStore.RefreshToken))
            {
                return false;
            }

            var response = await authApi.RefreshAsync(new RefreshTokenRequestDto
            {
                RefreshToken = tokenStore.RefreshToken
            }, cancellationToken);

            if (response.Code != 200 || response.Data is null)
            {
                Log.Warning("Refresh Token 失效：{Message}", response.Message);
                tokenStore.Clear();
                return false;
            }

            ApplyLoginResponse(response.Data);
            Log.Information("Access Token 已刷新");
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "刷新 Access Token 失败");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private void ApplyLoginResponse(LoginResponseDto data)
    {
        tokenStore.SetTokens(data.AccessToken, data.RefreshToken, data.ExpiresIn, data.UserInfo.FactoryId);
        sessionService.SetUser(data.UserInfo);
        sessionService.SetActionPermissions(data.UserInfo.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }
}

public static class ApiClientRegistration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IAuthApi CreateAuthApi(IConfiguration configuration, IApiTokenStore tokenStore, ILocalizationService localization)
    {
        var baseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("未配置 Api:BaseUrl");

        var handler = new AuthTokenHandler(tokenStore, localization)
        {
            InnerHandler = new HttpClientHandler()
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(configuration.GetValue("Api:TimeoutSeconds", 30))
        };

        return RestService.For<IAuthApi>(httpClient, new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions)
        });
    }

    public static IBarcodeApi CreateBarcodeApi(IConfiguration configuration, IApiTokenStore tokenStore, ILocalizationService localization)
    {
        var baseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("未配置 Api:BaseUrl");

        var handler = new AuthTokenHandler(tokenStore, localization)
        {
            InnerHandler = new HttpClientHandler()
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(configuration.GetValue("Api:TimeoutSeconds", 30))
        };

        return RestService.For<IBarcodeApi>(httpClient, new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions)
        });
    }
}
