namespace Example.Common.Models
{
    public class ExcelImportSummaryResult
    {
        public bool HasErrors { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int TotalCount { get; set; }
        public byte[]? ErrorFileBytes { get; set; }
    }
}
