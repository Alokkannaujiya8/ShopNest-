using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    private readonly ShopNestDbContext _dbContext;

    public PdfService(ShopNestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new ArgumentException("Order not found or deleted.");
        }

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII);

        // Minimalist compliant PDF 1.4 template generator
        writer.WriteLine("%PDF-1.4");
        writer.WriteLine("1 0 obj");
        writer.WriteLine("<< /Type /Catalog /Pages 2 0 R >>");
        writer.WriteLine("endobj");
        writer.WriteLine("2 0 obj");
        writer.WriteLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        writer.WriteLine("endobj");
        writer.WriteLine("3 0 obj");
        writer.WriteLine("<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 4 0 R >> >> /MediaBox [0 0 595 842] /Contents 5 0 R >>");
        writer.WriteLine("endobj");
        writer.WriteLine("4 0 obj");
        writer.WriteLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        writer.WriteLine("endobj");

        // Build Invoice Content
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 20 Tf");
        contentBuilder.AppendLine("70 780 Td");
        contentBuilder.AppendLine("(SHOPNEST INVOICE) Tj");
        contentBuilder.AppendLine("/F1 10 Tf");
        contentBuilder.AppendLine("0 -30 Td");
        contentBuilder.AppendLine($"((Order Number: #{order.OrderNumber})) Tj");
        contentBuilder.AppendLine("0 -20 Td");
        contentBuilder.AppendLine($"((Order Date: {order.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC)) Tj");
        contentBuilder.AppendLine("0 -20 Td");
        contentBuilder.AppendLine($"((Status: {order.Status})) Tj");
        contentBuilder.AppendLine("0 -40 Td");

        // Table Header
        contentBuilder.AppendLine("((Item Description              Qty      Price       Total)) Tj");
        contentBuilder.AppendLine("0 -20 Td");

        foreach (var item in order.Items)
        {
            var name = item.Product.Name;
            if (name.Length > 28) name = name.Substring(0, 25) + "...";
            name = name.PadRight(30);

            var line = $"({name}  {item.Quantity,-5}  {item.UnitPrice,-10:F2}  {(item.Quantity * item.UnitPrice),-10:F2}) Tj";
            contentBuilder.AppendLine(line);
            contentBuilder.AppendLine("0 -15 Td");
        }

        contentBuilder.AppendLine("0 -30 Td");
        contentBuilder.AppendLine("/F1 12 Tf");
        contentBuilder.AppendLine($"((Grand Total: {order.TotalAmount:C})) Tj");
        contentBuilder.AppendLine("ET");

        var textContent = contentBuilder.ToString();
        var contentBytes = Encoding.ASCII.GetBytes(textContent);

        writer.WriteLine("5 0 obj");
        writer.WriteLine($"<< /Length {contentBytes.Length} >>");
        writer.WriteLine("stream");
        writer.Flush();
        ms.Write(contentBytes, 0, contentBytes.Length);
        writer.WriteLine();
        writer.WriteLine("endstream");
        writer.WriteLine("endobj");

        writer.WriteLine("xref");
        writer.WriteLine("0 6");
        writer.WriteLine("0000000000 65535 f ");
        writer.WriteLine("0000000009 00000 n ");
        writer.WriteLine("0000000052 00000 n ");
        writer.WriteLine("0000000109 00000 n ");
        writer.WriteLine("0000000223 00000 n ");
        writer.WriteLine("0000000293 00000 n ");
        writer.WriteLine("trailer");
        writer.WriteLine("<< /Size 6 /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine("350");
        writer.WriteLine("%%EOF");
        writer.Flush();

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateSalesReportPdfAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _dbContext.Orders
            .Where(o => o.CreatedAtUtc >= startDate && o.CreatedAtUtc <= endDate && !o.IsDeleted)
            .ToListAsync();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII);

        writer.WriteLine("%PDF-1.4");
        writer.WriteLine("1 0 obj");
        writer.WriteLine("<< /Type /Catalog /Pages 2 0 R >>");
        writer.WriteLine("endobj");
        writer.WriteLine("2 0 obj");
        writer.WriteLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        writer.WriteLine("endobj");
        writer.WriteLine("3 0 obj");
        writer.WriteLine("<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 4 0 R >> >> /MediaBox [0 0 595 842] /Contents 5 0 R >>");
        writer.WriteLine("endobj");
        writer.WriteLine("4 0 obj");
        writer.WriteLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        writer.WriteLine("endobj");

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 20 Tf");
        contentBuilder.AppendLine("70 780 Td");
        contentBuilder.AppendLine("(SHOPNEST SALES REPORT) Tj");
        contentBuilder.AppendLine("/F1 10 Tf");
        contentBuilder.AppendLine("0 -30 Td");
        contentBuilder.AppendLine($"((Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})) Tj");
        contentBuilder.AppendLine("0 -30 Td");
        contentBuilder.AppendLine($"((Total Revenue: {totalRevenue:C})) Tj");
        contentBuilder.AppendLine("0 -20 Td");
        contentBuilder.AppendLine($"((Total Completed Orders: {totalOrders})) Tj");
        contentBuilder.AppendLine("0 -40 Td");
        contentBuilder.AppendLine("((End of Report)) Tj");
        contentBuilder.AppendLine("ET");

        var textContent = contentBuilder.ToString();
        var contentBytes = Encoding.ASCII.GetBytes(textContent);

        writer.WriteLine("5 0 obj");
        writer.WriteLine($"<< /Length {contentBytes.Length} >>");
        writer.WriteLine("stream");
        writer.Flush();
        ms.Write(contentBytes, 0, contentBytes.Length);
        writer.WriteLine();
        writer.WriteLine("endstream");
        writer.WriteLine("endobj");

        writer.WriteLine("xref");
        writer.WriteLine("0 6");
        writer.WriteLine("0000000000 65535 f ");
        writer.WriteLine("0000000009 00000 n ");
        writer.WriteLine("0000000052 00000 n ");
        writer.WriteLine("0000000109 00000 n ");
        writer.WriteLine("0000000223 00000 n ");
        writer.WriteLine("0000000293 00000 n ");
        writer.WriteLine("trailer");
        writer.WriteLine("<< /Size 6 /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine("350");
        writer.WriteLine("%%EOF");
        writer.Flush();

        return ms.ToArray();
    }
}
