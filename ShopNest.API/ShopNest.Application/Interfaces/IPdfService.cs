using System;
using System.Threading.Tasks;

namespace ShopNest.Application.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid orderId);
    Task<byte[]> GenerateSalesReportPdfAsync(DateTime startDate, DateTime endDate);
}
