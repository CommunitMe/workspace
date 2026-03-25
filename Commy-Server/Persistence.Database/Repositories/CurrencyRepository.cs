using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database.Repositories
{
	public class CurrencyRepository : ICurrencyRepository
	{
		private readonly CmeDbContext _db;

		public CurrencyRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<Currency> Create(Currency model)
		{
			var data = model.Adapt<Entities.Currency>();

			var result = _db.Currencies.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.Currency>();
		}

		public async Task Delete(Currency model)
		{
			ICriterion<Currency>? criterion = model.GetCriterion();
			var row = await _db.Events.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{nameof(Currency)}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Events.Remove(row);
			await _db.SaveChangesAsync();
		}

		public Task<List<Currency>> GetAll(ICriterion<Currency>? criterion = null)
		{
			var query = _db.Currencies.Select(m => m.Adapt<Domain.Models.Currency>()).AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Currency?> GetById(ICriterion<Currency> criterion)
		{
			var result = await _db.Currencies.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			return result.Adapt<Domain.Models.Currency>();
		}

		public async Task<Currency> Update(Currency model)
		{
			var dbEntity = await _db.Currencies.FindAsync(model.GetCriterion().GetKeys());

			if (dbEntity == null)
				throw new KeyNotFoundException($"Failed to find {nameof(Currency)} with given criterion");

			model.Adapt<Entities.Currency>().Adapt(dbEntity);
			await _db.SaveChangesAsync();

			return dbEntity.Adapt<Domain.Models.Currency>();
		}
	}
}
