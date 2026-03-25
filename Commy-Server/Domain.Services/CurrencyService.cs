using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
	public class CurrenciesService : ICurrencyService
	{
		private readonly ILogger<CurrenciesService> logger;
		private readonly IDatabaseTransaction db;

		public CurrenciesService(ILogger<CurrenciesService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<IEnumerable<Currency>> GetCurrencies()
		{
			List<Currency> CurrenciesList = await db.Currencies.GetAll();

			return CurrenciesList.AsEnumerable();
		}

		public async Task<Currency> GetCurrency(string currencyValue)
		{
			List<Currency> Currencies = await db.Currencies.GetAll(new CurrencyByValueCriterion { Value = currencyValue });

			if (Currencies.Count > 1)
			{
				logger.LogWarning("More than one Currency exist with the same value!");
				return Currencies.First();
			}

			if (Currencies.Count == 0)
			{
				throw new PersistantItemNotFoundException(typeof(Currency));
			}

			return Currencies.Single();
		}
	}
}
