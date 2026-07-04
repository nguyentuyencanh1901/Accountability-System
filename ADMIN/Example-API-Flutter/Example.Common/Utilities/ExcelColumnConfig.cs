using OfficeOpenXml;

namespace Example.Common.Utilities
{
    public class ExcelColumnConfig
    {
        public string Header { get; set; }
        public string PropertyName { get; set; }
        public Func<object, object> ValueFormatter { get; set; }
        public Action<ExcelRange> Style { get; set; }


    }
}
