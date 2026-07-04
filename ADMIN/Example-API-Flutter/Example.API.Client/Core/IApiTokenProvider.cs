namespace Example.API.Client.Core
{
    /// <summary>Lưu JWT sau login, tự gắn vào các request Authorize</summary>
    public interface IApiTokenProvider
    {
        string? Token { get; set; }
        void Clear();
    }
}
