using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services;

public sealed class ExcelService : IExcelService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        // Write Header
        for (int i = 0; i < properties.Length; i++)
        {
            sb.Append(EscapeCsv(properties[i].Name));
            if (i < properties.Length - 1) sb.Append(",");
        }
        sb.AppendLine();

        // Write Data Rows
        foreach (var item in data)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                var val = properties[i].GetValue(item);
                sb.Append(EscapeCsv(val?.ToString() ?? string.Empty));
                if (i < properties.Length - 1) sb.Append(",");
            }
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public List<T> ImportFromCsv<T>(byte[] csvBytes) where T : new()
    {
        var list = new List<T>();
        var csvText = Encoding.UTF8.GetString(csvBytes);
        var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1) return list;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var headerLine = lines[0];
        var headers = ParseCsvRow(headerLine);
        var propMap = new Dictionary<int, PropertyInfo>();

        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i].Trim();
            var prop = Array.Find(properties, p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                propMap[i] = prop;
            }
        }

        for (int row = 1; row < lines.Length; row++)
        {
            var cols = ParseCsvRow(lines[row]);
            if (cols.Count == 0) continue;

            var obj = new T();
            for (int i = 0; i < cols.Count; i++)
            {
                if (propMap.TryGetValue(i, out var prop))
                {
                    try
                    {
                        var val = ConvertValue(cols[i], prop.PropertyType);
                        prop.SetValue(obj, val);
                    }
                    catch
                    {
                        // Skip corrupted cell data silently
                    }
                }
            }
            list.Add(obj);
        }

        return list;
    }

    private static string EscapeCsv(string val)
    {
        if (val.Contains(",") || val.Contains("\"") || val.Contains("\n") || val.Contains("\r"))
        {
            return $"\"{val.Replace("\"", "\"\"")}\"";
        }
        return val;
    }

    private static List<string> ParseCsvRow(string rowText)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < rowText.Length; i++)
        {
            char c = rowText[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < rowText.Length && rowText[i + 1] == '"')
                {
                    sb.Append('"'); // Escaped double quote
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes; // Toggle quotes mode
                }
            }
            else if (c == ',' && !inQuotes)
            {
                list.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        list.Add(sb.ToString());
        return list;
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        return Convert.ChangeType(value, underlyingType);
    }
}
