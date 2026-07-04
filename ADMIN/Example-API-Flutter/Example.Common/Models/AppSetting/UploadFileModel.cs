namespace Example.Common.Models.AppSetting
{
    public class UploadFileModel
    {
        public string EndPoint { get; set; }
        public int Port { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }        
        public bool IsSSL { get; set; }
    }
}
