using System.Collections.Generic;

namespace ShopNest.Application.Interfaces;

public interface IExcelService
{
    byte[] ExportToCsv<T>(IEnumerable<T> data);
    List<T> ImportFromCsv<T>(byte[] csvBytes) where T : new();
}
