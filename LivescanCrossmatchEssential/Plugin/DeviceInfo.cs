namespace Livescan.Scanners.DriverEssential.Plugin
{
    using Livescan.Scanners.DriverEssential.Sdk;

    internal class DeviceInfo
    {
        public DeviceInfo(int index, LScanDeviceInfo device)
        {
            this.Index = index;
            this.SerialNumber = device.serialNumber.ToUpper();
            this.TypeName = device.typeName.ToUpper();
            this.InterfaceType = device.interfaceType;
        }

        public int Index { get; private set; }

        public string SerialNumber { get; private set; }

        public string TypeName { get; private set; }

        public string InterfaceType { get; private set; }

    }
}
