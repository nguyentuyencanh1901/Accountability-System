namespace Example.Common.Utilities
{
    public class ExcelImportColumnConfig
    {
        public string Header { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public Type PropertyType { get; set; } = typeof(string);
        public bool Required { get; set; }
        public int? MaxLength { get; set; }
        public double ColumnWidth { get; set; } = 20;
        public float FontSize { get; set; } = 11;
        public Func<object?, string?>? CustomValidate { get; set; }
    }
}
