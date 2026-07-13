using Microsoft.Extensions.Configuration;

namespace WF.MES.WPF.DeviceAdapters;

public sealed class DeviceAdapterRegistry
{
    public IEquipmentTester EquipmentTester { get; }

    public DeviceAdapterRegistry(IConfiguration configuration)
    {
        // 当前仅 Mock；配置项预留给后续真实测试仪适配器
        _ = configuration["Devices:EquipmentTester"];
        EquipmentTester = new MockEquipmentTester();
    }
}
