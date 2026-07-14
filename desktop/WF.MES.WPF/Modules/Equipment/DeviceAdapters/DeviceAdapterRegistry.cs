using Microsoft.Extensions.Configuration;

namespace WF.MES.WPF.Modules.Equipment.DeviceAdapters;

public sealed class DeviceAdapterRegistry
{
    public IEquipmentTester EquipmentTester { get; }

    public DeviceAdapterRegistry(IConfiguration configuration)
    {
        // 真实测试仪适配器接入前固定 Mock；configuration 预留解析 Devices:EquipmentTester
        _ = configuration;
        EquipmentTester = new MockEquipmentTester();
    }
}
