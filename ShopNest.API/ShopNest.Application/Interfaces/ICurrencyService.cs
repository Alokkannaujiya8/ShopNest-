using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNest.Application.Interfaces;

public interface ICurrencyService
{
    decimal Convert(decimal amount, string fromCurrency, string toCurrency);
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync();
}
