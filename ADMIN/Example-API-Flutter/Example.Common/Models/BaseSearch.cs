namespace Example.Common.Models
{
    public class BaseSearch
    {
        public long Id { get; set; }
        public int? Status { get; set; }
        public string? Keyword { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
