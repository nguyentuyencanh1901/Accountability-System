namespace Example.Common.Models.AppSetting
{
    public class ElasticSearchModel
    {
        public bool EnableLogElasticsearch { get; set; } = false;
        public string Uri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CertificateFile { get; set; }
        public string CertificatePassword { get; set; }
    }
}
