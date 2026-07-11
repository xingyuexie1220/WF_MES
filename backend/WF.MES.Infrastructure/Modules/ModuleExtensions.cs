using Microsoft.Extensions.DependencyInjection;

namespace WF.MES.Infrastructure.Modules;

public static class SystemModuleExtensions
{
    public static IServiceCollection AddWfSystemModule(this IServiceCollection services)
    {
        // System 模块服务已在 DependencyInjection 注册
        return services;
    }
}

public static class MasterDataModuleExtensions
{
    public static IServiceCollection AddWfMasterDataModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.MasterData.IMasterDataScaffoldService,
            WF.MES.Infrastructure.Services.MasterDataScaffoldService>();
        return services;
    }
}

public static class ProductionModuleExtensions
{
    public static IServiceCollection AddWfProductionModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.Production.IProductionScaffoldService,
            WF.MES.Infrastructure.Services.ProductionScaffoldService>();
        return services;
    }
}

public static class WarehouseModuleExtensions
{
    public static IServiceCollection AddWfWarehouseModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.Warehouse.IWarehouseScaffoldService,
            WF.MES.Infrastructure.Services.WarehouseScaffoldService>();
        return services;
    }
}

public static class DashboardModuleExtensions
{
    public static IServiceCollection AddWfDashboardModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.Dashboard.IDashboardScaffoldService,
            WF.MES.Infrastructure.Services.DashboardScaffoldService>();
        return services;
    }
}

public static class EquipmentModuleExtensions
{
    public static IServiceCollection AddWfEquipmentModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.Equipment.IEquipmentScaffoldService,
            WF.MES.Infrastructure.Services.EquipmentScaffoldService>();
        return services;
    }
}

public static class BarcodeModuleExtensions
{
    public static IServiceCollection AddWfBarcodeModule(this IServiceCollection services)
    {
        services.AddScoped<WF.MES.Application.Barcode.IBarcodeCustomerService,
            WF.MES.Infrastructure.Services.Barcode.BarcodeCustomerService>();
        services.AddScoped<WF.MES.Application.Barcode.IBarcodeScaffoldService,
            WF.MES.Infrastructure.Services.Barcode.BarcodeScaffoldService>();
        return services;
    }
}

public static class WfModuleRegistration
{
    public static IServiceCollection AddWfModules(this IServiceCollection services)
    {
        services.AddWfSystemModule();
        services.AddWfMasterDataModule();
        services.AddWfProductionModule();
        services.AddWfWarehouseModule();
        services.AddWfDashboardModule();
        services.AddWfEquipmentModule();
        services.AddWfBarcodeModule();
        return services;
    }
}
