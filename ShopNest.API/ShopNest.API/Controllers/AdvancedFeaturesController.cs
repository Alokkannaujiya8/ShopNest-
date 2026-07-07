using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;
using ShopNest.Application.Interfaces;
using Asp.Versioning;

namespace ShopNest.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/advanced")]
public sealed class AdvancedFeaturesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAiRecommendationService _recommendationService;
    private readonly IPdfService _pdfService;
    private readonly IExcelService _excelService;
    private readonly ICurrencyService _currencyService;
    private readonly Microsoft.Extensions.Localization.IStringLocalizer<AdvancedFeaturesController> _localizer;

    public AdvancedFeaturesController(
        IMediator mediator,
        IAiRecommendationService recommendationService,
        IPdfService pdfService,
        IExcelService excelService,
        ICurrencyService currencyService,
        Microsoft.Extensions.Localization.IStringLocalizer<AdvancedFeaturesController> localizer)
    {
        _mediator = mediator;
        _recommendationService = recommendationService;
        _pdfService = pdfService;
        _excelService = excelService;
        _currencyService = currencyService;
        _localizer = localizer;
    }

    // 0. Localization verification
    [HttpGet("localize")]
    public ActionResult<ApiResponse<string>> GetLocalizedWelcome()
    {
        var welcomeMessage = _localizer["Welcome"];
        return Ok(ApiResponse<string>.SuccessResult(welcomeMessage));
    }

    // 1. AI Product Recommendations
    [HttpGet("recommendations/user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetRecommendationsForUser(Guid userId, [FromQuery] int count = 10)
    {
        var result = await _recommendationService.GetRecommendationsForUserAsync(userId, count);
        return Ok(ApiResponse<List<ProductDto>>.SuccessResult(result));
    }

    [HttpGet("recommendations/frequently-bought/{productId}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetFrequentlyBoughtTogether(Guid productId, [FromQuery] int count = 5)
    {
        var result = await _recommendationService.GetFrequentlyBoughtTogetherAsync(productId, count);
        return Ok(ApiResponse<List<ProductDto>>.SuccessResult(result));
    }

    [HttpGet("recommendations/similar/{productId}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetSimilarProducts(Guid productId, [FromQuery] int count = 5)
    {
        var result = await _recommendationService.GetSimilarProductsAsync(productId, count);
        return Ok(ApiResponse<List<ProductDto>>.SuccessResult(result));
    }

    [HttpGet("recommendations/trending")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetTrendingProducts([FromQuery] int count = 10)
    {
        var result = await _recommendationService.GetTrendingProductsAsync(count);
        return Ok(ApiResponse<List<ProductDto>>.SuccessResult(result));
    }

    [HttpGet("recommendations/popular")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetPopularProducts([FromQuery] int count = 10)
    {
        var result = await _recommendationService.GetPopularProductsAsync(count);
        return Ok(ApiResponse<List<ProductDto>>.SuccessResult(result));
    }

    // 2. Global Search
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<GlobalSearchResultDto>>> GlobalSearch([FromQuery] string query, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GlobalSearchQuery(query, limit), cancellationToken);
        return Ok(ApiResponse<GlobalSearchResultDto>.SuccessResult(result));
    }

    // 3. Product Comparison
    [HttpGet("compare")]
    public async Task<ActionResult<ApiResponse<List<ProductCompareResultDto>>>> CompareProducts([FromQuery] List<Guid> ids, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductComparisonQuery(ids), cancellationToken);
        return Ok(ApiResponse<List<ProductCompareResultDto>>.SuccessResult(result));
    }

    // 4. Order Timelines
    [HttpGet("timeline/order/{orderId}")]
    public async Task<ActionResult<ApiResponse<List<TimelineEventDto>>>> GetOrderTimeline(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderTimelineQuery(orderId), cancellationToken);
        return Ok(ApiResponse<List<TimelineEventDto>>.SuccessResult(result));
    }

    // 5. Restore soft deleted entities (Admin only)
    [HttpPost("restore")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreRecord([FromBody] RestoreEntityCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResult(result));
    }

    // 6. PDF Invoice & Report Downloads
    [HttpGet("invoice/{orderId}/download")]
    public async Task<IActionResult> DownloadInvoice(Guid orderId)
    {
        var bytes = await _pdfService.GenerateInvoicePdfAsync(orderId);
        return File(bytes, "application/pdf", $"invoice_{orderId}.pdf");
    }

    [HttpGet("reports/sales/download")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DownloadSalesReport([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var bytes = await _pdfService.GenerateSalesReportPdfAsync(start, end);
        return File(bytes, "application/pdf", $"sales_report_{start:yyyyMMdd}_to_{end:yyyyMMdd}.pdf");
    }

    // 7. Multi-currency Convert & Rates
    [HttpGet("currency/convert")]
    public ActionResult<ApiResponse<decimal>> ConvertCurrency([FromQuery] decimal amount, [FromQuery] string from, [FromQuery] string to)
    {
        var result = _currencyService.Convert(amount, from, to);
        return Ok(ApiResponse<decimal>.SuccessResult(result));
    }

    [HttpGet("currency/rates")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetCurrencyRates()
    {
        var result = await _currencyService.GetExchangeRatesAsync();
        return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResult(result));
    }

    // 8. Excel/CSV Import/Export
    [HttpGet("export/products/csv")]
    public async Task<IActionResult> ExportProductsCsv()
    {
        // Simple mock product list export for CSV validation
        var products = new List<ProductExportRow>
        {
            new("PROD001", "Premium Wireless Earbuds", 149.99m, 120, "Electronics"),
            new("PROD002", "Mechanical Gaming Keyboard", 89.99m, 45, "Electronics"),
            new("PROD003", "Ergonomic Office Chair", 249.99m, 15, "Furniture")
        };

        var bytes = _excelService.ExportToCsv(products);
        return File(bytes, "text/csv", "products_export.csv");
    }

    [HttpPost("import/products/csv")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<ProductExportRow>>>> ImportProductsCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<List<ProductExportRow>>.FailureResult("No file uploaded."));
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var rows = _excelService.ImportFromCsv<ProductExportRow>(bytes);
        return Ok(ApiResponse<List<ProductExportRow>>.SuccessResult(rows));
    }
}

public record ProductExportRow(string Sku, string Name, decimal Price, int Stock, string Category)
{
    public ProductExportRow() : this(string.Empty, string.Empty, 0m, 0, string.Empty) { }
}
