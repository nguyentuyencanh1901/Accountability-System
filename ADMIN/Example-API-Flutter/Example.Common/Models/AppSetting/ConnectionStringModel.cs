namespace Example.Common.Models.AppSetting
{
    public class ConnectionStringsModel
    {
        public ConnectionStringModel ConnectionStrings { get; set; }
    }
    public class ConnectionStringModel
    {
        public string CommonConnectionString { get; set; }
        public string MasterConnectionString { get; set; }
        public string SlaveConnectionString { get; set; }
    }
}
