namespace Example.Common.Models
{
    public class ExcelImportRowResult<T> where T : class, new()
    {
        public int RowNumber { get; set; }
        public T Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool IsValid => Errors.Count == 0;
        public string ErrorMessage => string.Join("; ", Errors);
    }
}
