using FluentValidation;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using SqlSugar;
using WF.MES.Core;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Api;
using WF.MES.Infrastructure.Data;
using WF.MES.Infrastructure.Services;
using WF.MES.Infrastructure.Localization;
using WF.MES.Infrastructure.Services.Barcode;
using WF.MES.Infrastructure.Services.Printing;
using WF.MES.Infrastructure.Validation.Barcode;
using WF.MES.Infrastructure.Validation.Printing;
using WF.MES.Infrastructure.Validation.System;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure;

/// <summary>Infrastructure 层 Prism DI 注册入口。</summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>注册 API 客户端、数据库、业务 Service 与 FluentValidation。</summary>
    public static void RegisterServices(IContainerRegistry containerRegistry, IConfiguration configuration, IAppVersion appVersion)
    {
        containerRegistry.RegisterInstance(configuration);
        containerRegistry.RegisterInstance(appVersion);

        containerRegistry.RegisterSingleton<ILocalizationService, JsonLocalizationService>();
        containerRegistry.RegisterSingleton<IApiTokenStore, ApiTokenStore>();
        containerRegistry.RegisterSingleton<IAuthApi>(container =>
            ApiClientRegistration.CreateAuthApi(
                container.Resolve<IConfiguration>(),
                container.Resolve<IApiTokenStore>(),
                container.Resolve<ILocalizationService>()));
        containerRegistry.RegisterSingleton<IBarcodeApi>(container =>
            ApiClientRegistration.CreateBarcodeApi(
                container.Resolve<IConfiguration>(),
                container.Resolve<IApiTokenStore>(),
                container.Resolve<ILocalizationService>()));

        containerRegistry.RegisterSingleton<ISqlSugarClient>(_ => SqlSugarSetup.CreateClient(configuration));
        containerRegistry.RegisterSingleton<IDatabaseHealthService, DatabaseHealthService>();
        containerRegistry.RegisterSingleton<IApiHealthService, ApiHealthService>();
        containerRegistry.RegisterSingleton<IAuthService, ApiAuthService>();
        containerRegistry.RegisterSingleton<ISessionService, SessionService>();
        containerRegistry.RegisterSingleton<IMenuPermissionService, ApiMenuPermissionService>();
        containerRegistry.RegisterSingleton<IMenuActionAuthorization, MenuActionAuthorization>();
        containerRegistry.RegisterSingleton<IUpdateService, UpdateService>();
        containerRegistry.RegisterSingleton<ICustomerService, ApiCustomerService>();
        containerRegistry.RegisterSingleton<IMaterialBarcodeRuleService, MaterialBarcodeRuleService>();
        containerRegistry.RegisterSingleton<ISerialNumberFormatter, SerialNumberFormatter>();
        containerRegistry.RegisterSingleton<IBarcodeBuilder, BarcodeBuilder>();
        containerRegistry.RegisterSingleton<IBarcodeGenerateService, BarcodeGenerateService>();
        containerRegistry.RegisterSingleton<IBarcodeGenerateRecordService, BarcodeGenerateRecordService>();
        containerRegistry.RegisterSingleton<IBarcodeQaReviewService, BarcodeQaReviewService>();
        containerRegistry.RegisterSingleton<ILabelPrintService, BarTenderLabelPrintService>();
        containerRegistry.RegisterSingleton<ISopService, SopService>();

        RegisterValidators(containerRegistry);
    }

    private static void RegisterValidators(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<IValidator<CustomerEditDto>, CustomerEditValidator>();
        containerRegistry.Register<IValidator<MaterialRuleEditDto>, MaterialRuleEditValidator>();
        containerRegistry.Register<IValidator<BarcodeGenerateRequestDto>, BarcodeGenerateRequestValidator>();
        containerRegistry.Register<IValidator<BarcodeQaReviewSaveAttachmentsDto>, BarcodeQaReviewSaveAttachmentsValidator>();
        containerRegistry.Register<IValidator<BarcodeQaReviewRejectDto>, BarcodeQaReviewRejectValidator>();
        containerRegistry.Register<IValidator<LabelPrintRequestDto>, LabelPrintRequestValidator>();
        containerRegistry.Register<IValidator<PasswordChangeDto>, PasswordChangeValidator>();
    }
}
