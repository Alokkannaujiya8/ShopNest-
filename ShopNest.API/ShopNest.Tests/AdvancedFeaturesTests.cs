using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ShopNest.Application.Interfaces;
using ShopNest.Infrastructure.Services;
using Xunit;

namespace ShopNest.Tests;

public sealed class AdvancedFeaturesTests
{
    [Fact]
    public void CurrencyService_Convert_ShouldConvertBaseAndTargetCurrenciesCorrectly()
    {
        // Arrange
        var service = new CurrencyService();

        // Act
        // From USD (base) to INR (83.5)
        var usdToInr = service.Convert(10.0m, "USD", "INR");
        // From INR to USD
        var inrToUsd = service.Convert(83.5m, "INR", "USD");
        // From EUR (0.92) to INR
        var eurToInr = service.Convert(1.0m, "EUR", "INR");

        // Assert
        Assert.Equal(835.0m, usdToInr);
        Assert.Equal(1.0m, inrToUsd);
        Assert.Equal((1.0m / 0.92m) * 83.5m, eurToInr, 4);
    }

    [Fact]
    public void ExcelService_ExportAndImport_ShouldRoundtripCsvCorrectly()
    {
        // Arrange
        var service = new ExcelService();
        var rows = new List<TestProductRow>
        {
            new("SKU001", "Smartphone, Flagship", 999.99m, 50),
            new("SKU002", "Standard \"Keyboard\"", 49.99m, 120),
            new("SKU003", "Office Chair Ergonomic", 199.90m, 12)
        };

        // Act
        var csvBytes = service.ExportToCsv(rows);
        Assert.NotNull(csvBytes);
        Assert.True(csvBytes.Length > 0);

        var importedRows = service.ImportFromCsv<TestProductRow>(csvBytes);

        // Assert
        Assert.Equal(3, importedRows.Count);
        Assert.Equal("SKU001", importedRows[0].Sku);
        Assert.Equal("Smartphone, Flagship", importedRows[0].Name);
        Assert.Equal(999.99m, importedRows[0].Price);
        Assert.Equal(50, importedRows[0].Stock);

        Assert.Equal("SKU002", importedRows[1].Sku);
        Assert.Equal("Standard \"Keyboard\"", importedRows[1].Name);
        Assert.Equal(49.99m, importedRows[1].Price);

        Assert.Equal("SKU003", importedRows[2].Sku);
        Assert.Equal("Office Chair Ergonomic", importedRows[2].Name);
        Assert.Equal(12, importedRows[2].Stock);
    }
}

public record TestProductRow(string Sku, string Name, decimal Price, int Stock)
{
    public TestProductRow() : this(string.Empty, string.Empty, 0m, 0) { }
}
