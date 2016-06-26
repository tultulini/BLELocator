namespace BLELocator.Core
{
    public class DeviceDetails
    {
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public override string ToString()
        {
            return string.Format("Name: {0}, MacAddress{1}", Name, MacAddress);
        }
    }
}