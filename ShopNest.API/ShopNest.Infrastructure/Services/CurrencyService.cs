using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNest.Application.Interfaces;

namespace ShopNest.Infrastructure.Services;

public sealed class CurrencyService : ICurrencyService
{
    private static readonly Dictionary<string, decimal> DefaultRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "USD", 1.0m },
        { "INR", 83.5m },
        { "EUR", 0.92m }
    };

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        if (!DefaultRates.TryGetValue(fromCurrency, out var fromRate) || 
            !DefaultRates.TryGetValue(toCurrency, out var toRate))
        {
            throw new ArgumentException($"Unsupported currency conversion from '{fromCurrency}' to '{toCurrency}'.");
        }

        // Convert to base (USD) first, then convert to target currency
        decimal amountInUsd = amount / fromRate;
        return amountInUsd * toRate;
    }

    public Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
    {
        return Task.FromResult(new Dictionary<string, decimal>(DefaultRates, StringComparer.OrdinalIgnoreCase));
    }
}
