using Example.Common.Models;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;
using System.Reflection;

namespace Example.Common.Utilities.Helper
{
    public static class ExcelImportHelper
    {
        public const string ErrorColumnHeader = "Lỗi";
        private const int TitleRow = 1;
        private const int HeaderRow = 2;
        private const int DataStartRow = 3;

        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };
        private static readonly string[] AllowedContentTypes =
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel"
        };

        public static bool IsValidExcelFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType)
                && !AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static byte[] GenerateImportTemplate(
            string sheetName,
            string title,
            List<ExcelImportColumnConfig> columns)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add(sheetName);

            ApplyTitleRow(sheet, title, columns.Count);
            ApplyHeaderRow(sheet, columns, includeErrorColumn: false);
            ApplyColumnWidths(sheet, columns, includeErrorColumn: false);

            sheet.View.FreezePanes(DataStartRow, 1);
            return package.GetAsByteArray();
        }

        public static List<ExcelImportRowResult<T>> ReadAndValidate<T>(
            Stream stream,
            List<ExcelImportColumnConfig> columns) where T : class, new()
        {
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("File Excel không có sheet dữ liệu.");

            var columnIndexMap = BuildColumnIndexMap(sheet, columns);
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);

            var results = new List<ExcelImportRowResult<T>>();
            var lastRow = sheet.Dimension?.End.Row ?? HeaderRow;

            for (var row = DataStartRow; row <= lastRow; row++)
            {
                if (IsEmptyRow(sheet, row, columnIndexMap))
                {
                    continue;
                }

                var rowResult = new ExcelImportRowResult<T> { RowNumber = row };

                foreach (var column in columns)
                {
                    if (!columnIndexMap.TryGetValue(column.Header, out var colIndex))
                    {
                        rowResult.Errors.Add($"Không tìm thấy cột '{column.Header}'");
                        continue;
                    }

                    var rawValue = sheet.Cells[row, colIndex].Text?.Trim() ?? string.Empty;
                    var (value, error) = ParseValue(rawValue, column);

                    if (!string.IsNullOrEmpty(error))
                    {
                        rowResult.Errors.Add(error);
                        continue;
                    }

                    if (properties.TryGetValue(column.PropertyName, out var property) && property.CanWrite)
                    {
                        property.SetValue(rowResult.Data, value);
                    }

                    var customError = column.CustomValidate?.Invoke(value);
                    if (!string.IsNullOrWhiteSpace(customError))
                    {
                        rowResult.Errors.Add(customError);
                    }
                }

                results.Add(rowResult);
            }

            return results;
        }

        public static byte[] BuildErrorReport<T>(
            List<ExcelImportRowResult<T>> rows,
            string sheetName,
            string title,
            List<ExcelImportColumnConfig> columns) where T : class, new()
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add(sheetName);
            var totalColumns = columns.Count + 1;

            ApplyTitleRow(sheet, title, totalColumns);
            ApplyHeaderRow(sheet, columns, includeErrorColumn: true);
            ApplyColumnWidths(sheet, columns, includeErrorColumn: true);

            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p);

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var excelRow = DataStartRow + i;

                for (var col = 0; col < columns.Count; col++)
                {
                    var column = columns[col];
                    var cell = sheet.Cells[excelRow, col + 1];

                    if (properties.TryGetValue(column.PropertyName, out var property))
                    {
                        cell.Value = property.GetValue(row.Data);
                    }

                    ApplyDataCellStyle(cell, column.FontSize);
                }

                var errorCell = sheet.Cells[excelRow, totalColumns];
                errorCell.Value = row.ErrorMessage;
                ApplyDataCellStyle(errorCell, 11);

                if (!row.IsValid)
                {
                    errorCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    errorCell.Style.Font.Bold = true;
                }
            }

            sheet.View.FreezePanes(DataStartRow, 1);
            return package.GetAsByteArray();
        }

        private static Dictionary<string, int> BuildColumnIndexMap(
            ExcelWorksheet sheet,
            List<ExcelImportColumnConfig> columns)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var lastCol = sheet.Dimension?.End.Column ?? columns.Count;

            for (var col = 1; col <= lastCol; col++)
            {
                var header = sheet.Cells[HeaderRow, col].Text?.Trim();
                if (string.IsNullOrWhiteSpace(header) || header.Equals(ErrorColumnHeader, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                map[header] = col;
            }

            var missingColumns = columns
                .Where(c => !map.ContainsKey(c.Header))
                .Select(c => c.Header)
                .ToList();

            if (missingColumns.Any())
            {
                throw new InvalidOperationException($"File Excel thiếu các cột: {string.Join(", ", missingColumns)}");
            }

            return map;
        }

        private static bool IsEmptyRow(ExcelWorksheet sheet, int row, Dictionary<string, int> columnIndexMap)
        {
            return columnIndexMap.Values.All(colIndex =>
            {
                var text = sheet.Cells[row, colIndex].Text;
                return string.IsNullOrWhiteSpace(text);
            });
        }

        private static (object? Value, string? Error) ParseValue(string rawValue, ExcelImportColumnConfig column)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                if (column.Required)
                {
                    return (null, $"{column.Header} không được để trống");
                }

                return (GetDefaultValue(column.PropertyType), null);
            }

            if (column.MaxLength.HasValue && rawValue.Length > column.MaxLength.Value)
            {
                return (null, $"{column.Header} không được vượt quá {column.MaxLength} ký tự");
            }

            try
            {
                object? parsedValue = column.PropertyType switch
                {
                    Type t when t == typeof(string) => rawValue,
                    Type t when t == typeof(int) => int.Parse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture),
                    Type t when t == typeof(long) => long.Parse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture),
                    Type t when t == typeof(decimal) => decimal.Parse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture),
                    Type t when t == typeof(double) => double.Parse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture),
                    Type t when t == typeof(bool) => ParseBoolean(rawValue),
                    Type t when t == typeof(DateTime) => DateTime.Parse(rawValue, CultureInfo.InvariantCulture),
                    _ => Convert.ChangeType(rawValue, column.PropertyType, CultureInfo.InvariantCulture)
                };

                return (parsedValue, null);
            }
            catch
            {
                return (null, $"{column.Header} không đúng định dạng");
            }
        }

        private static bool ParseBoolean(string rawValue)
        {
            if (bool.TryParse(rawValue, out var boolValue))
            {
                return boolValue;
            }

            return rawValue switch
            {
                "1" or "có" or "Có" or "CO" or "co" => true,
                "0" or "không" or "Không" or "KHONG" or "khong" => false,
                _ => throw new FormatException()
            };
        }

        private static object? GetDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static void ApplyTitleRow(ExcelWorksheet sheet, string title, int totalColumns)
        {
            sheet.Cells[TitleRow, 1, TitleRow, totalColumns].Merge = true;
            var titleCell = sheet.Cells[TitleRow, 1];
            titleCell.Value = title;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.Size = 14;
            titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Row(TitleRow).Height = 28;
        }

        private static void ApplyHeaderRow(
            ExcelWorksheet sheet,
            List<ExcelImportColumnConfig> columns,
            bool includeErrorColumn)
        {
            for (var col = 0; col < columns.Count; col++)
            {
                var cell = sheet.Cells[HeaderRow, col + 1];
                cell.Value = columns[col].Header;
                ApplyHeaderCellStyle(cell);
            }

            if (includeErrorColumn)
            {
                var errorHeaderCell = sheet.Cells[HeaderRow, columns.Count + 1];
                errorHeaderCell.Value = ErrorColumnHeader;
                ApplyHeaderCellStyle(errorHeaderCell);
                errorHeaderCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MistyRose);
            }

            sheet.Row(HeaderRow).Height = 22;
        }

        private static void ApplyHeaderCellStyle(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 11;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private static void ApplyDataCellStyle(ExcelRange cell, float fontSize)
        {
            cell.Style.Font.Size = fontSize;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private static void ApplyColumnWidths(
            ExcelWorksheet sheet,
            List<ExcelImportColumnConfig> columns,
            bool includeErrorColumn)
        {
            for (var col = 0; col < columns.Count; col++)
            {
                sheet.Column(col + 1).Width = columns[col].ColumnWidth;
            }

            if (includeErrorColumn)
            {
                sheet.Column(columns.Count + 1).Width = 50;
            }
        }
    }
}
