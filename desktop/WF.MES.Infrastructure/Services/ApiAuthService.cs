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
                return Fail(ApiErrorHelper.ResolveMessage(
                    localization, response.MessageCode, response.Message, "auth.invalid_credentials"));
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
            return Ok(response.Data.UserInfo);
        }
        catch (Exception ex)
        {
            return Fail(ApiErrorHelper.ToUserMessage(ex, localization, _apiBaseUrl));
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
                return Fail(ApiErrorHelper.ResolveMessage(
                    localization, response.MessageCode, response.Message, "ui.factory.selectFailed"));
            }

            ApplyLoginResponse(response.Data);
            return Ok(response.Data.UserInfo);
        }
        catch (Exception ex)
        {
            return Fail(ApiErrorHelper.ToUserMessage(ex, localization, _apiBaseUrl));
        }
    }

    public async Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenStore.AccessToken))
        {
            return false;
        }

        if (tokenStore.AccessTokenExpiresAt is { } expiresAt && expiresAt <= DateTimeOffset.UtcNow)
        {
            Log.Information("Access Token 已过期，需重新登录");
            return false;
        }

        return await PingSessionAsync(cancellationToken);
    }

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

        var user = ApiResponseHelper.EnsureData(response, "err.changePasswordFailed");
        sessionService.SetUser(user);
        sessionService.SetActionPermissions(user.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
        return user;
    }

    private async Task<bool> PingSessionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await authApi.GetInfoAsync(cancellationToken);
            if (response.Code != 200 || response.Data is null)
            {
                if (ApiResponseHelper.IsSessionReplaced(response))
                {
                    Log.Information("会话已在其他设备登录");
                }

                return false;
            }

            sessionService.SetUser(response.Data);
            sessionService.SetActionPermissions(response.Data.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
            return true;
        }
        catch (ApiException apiEx)
        {
            if (ApiErrorHelper.TryParseApiResult(apiEx, out var messageCode, out _)
                && ApiResponseHelper.IsSessionReplacedCode(messageCode))
            {
                Log.Information("会话已在其他设备登录");
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyLoginResponse(LoginResponseDto data)
    {
        tokenStore.SetAccessToken(data.AccessToken, data.ExpiresIn, data.UserInfo.FactoryId);
        sessionService.SetUser(data.UserInfo);
        sessionService.SetActionPermissions(data.UserInfo.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static LoginResultDto Ok(UserInfoDto? user) => new()
    {
        Success = true,
        User = user
    };

    private static LoginResultDto Fail(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
