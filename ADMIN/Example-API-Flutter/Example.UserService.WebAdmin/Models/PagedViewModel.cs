namespace Example.UserService.WebAdmin.Models
{
    public class PagedViewModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public long TotalItems { get; set; }
        public string? ErrorMessage { get; set; }

        public virtual string BuildPageUrl(int pageIndex) => $"?pageIndex={pageIndex}";
    }
}
