using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Utilities.Helper
{
    public static class ExcelHelper
    {
        public static byte[] Export<T>(
     List<T> data,
     string sheetName,
     string title,
     List<ExcelColumnConfig> columns)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add(sheetName);

            // ✅ cache properties (tránh gọi nhiều lần)
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);

            // ✅ TITLE
            sheet.Cells[1, 1, 1, columns.Count].Merge = true;
            sheet.Cells[1, 1].Value = title;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 14;
            sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int headerRow = 2;

            // ✅ HEADER
            for (int col = 0; col < columns.Count; col++)
            {
                var cell = sheet.Cells[headerRow, col + 1];

                cell.Value = columns[col].Header;
                cell.Style.Font.Bold = true;

                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // ✅ border header
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // ✅ căn giữa header
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // ✅ DATA
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    var column = columns[col];
                    var cell = sheet.Cells[row + headerRow + 1, col + 1];

                    if (!properties.TryGetValue(column.PropertyName, out var prop))
                        continue;

                    var rawValue = prop.GetValue(data[row]);

                    var value = column.ValueFormatter != null
                        ? column.ValueFormatter(rawValue)
                        : rawValue;

                    cell.Value = value;

                    // ✅ border từng ô
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    // ✅ apply style custom
                    column.Style?.Invoke(cell);
                }
            }

            sheet.Cells.AutoFitColumns();

            int totalRow = data.Count + headerRow;
            int totalCol = columns.Count;

            // ✅ FREEZE HEADER
            sheet.View.FreezePanes(headerRow + 1, 1);

            return package.GetAsByteArray();
        }
    }
}
