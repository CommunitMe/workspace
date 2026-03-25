using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface ICurrencyService
	{
		Task<Currency> GetCurrency(string currency);
		Task<IEnumerable<Currency>> GetCurrencies();
	}
}
