namespace Example.API.Client.Core
{
    public class ApiTokenProvider : IApiTokenProvider
    {
        public string? Token { get; set; }
        public void Clear() => Token = null;
    }
}
