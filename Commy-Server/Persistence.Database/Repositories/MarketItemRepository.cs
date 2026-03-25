using Domain.Abstractions.Common;
using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Database.Repositories
{
	public class MarketItemRepository : IMarketItemRepository
	{
		private readonly CmeDbContext _db;

		public MarketItemRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<MarketItem> Create(MarketItem model)
		{
			var data = model.Adapt<Entities.MarketItem>();

			var result = _db.MarketItems.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.MarketItem>();
		}

		public async Task Delete(MarketItem model)
		{
			ICriterion<MarketItem>? criterion = model.GetCriterion();
			var row = await _db.MarketItems.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(MarketItem).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			row.Tags.Clear();
			_db.Remove(row);
			await _db.SaveChangesAsync();
		}

		public Task<List<MarketItem>> GetAll(ICriterion<MarketItem>? criterion)
		{
			var query = _db.MarketItems.Include(m => m.PriceCurrencyNavigation)
						.Include(m => m.Tags)
						.Select(m => m.Adapt<Domain.Models.MarketItem>())
						.AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<MarketItem?> GetById(ICriterion<MarketItem> criterion)
		{
			var result = await _db.MarketItems.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			_db.Entry(result).Collection(m => m.Tags).Load();
			return result.Adapt<Domain.Models.MarketItem>();
		}

		public async Task<MarketItem> Update(MarketItem model)
		{
			var entity = await _db.MarketItems.FindAsync(model.GetCriterion().GetKeys());

			if (entity == null)
				throw new KeyNotFoundException($"Failed to find {nameof(MarketItem)} with given criterion");

			var updatedEntity = model.Adapt<Entities.MarketItem>();

			if (updatedEntity.RelevantCommunity == entity.RelevantCommunity)
				updatedEntity.RelevantCommunityNavigation = entity.RelevantCommunityNavigation;
			else
			{
				var navigationEntity = _db.Communities.Find(updatedEntity.RelevantCommunity);
				if (navigationEntity == null)
					throw new PersistantItemNotFoundException(typeof(Community));
				updatedEntity.RelevantCommunityNavigation = navigationEntity;
			}

			if (updatedEntity.PriceCurrency == entity.PriceCurrency)
				updatedEntity.PriceCurrencyNavigation = entity.PriceCurrencyNavigation;
			else
			{
				var navigationEntity = _db.Currencies.Find(updatedEntity.PriceCurrency);
				if (navigationEntity == null)
					throw new PersistantItemNotFoundException(typeof(Currency));
				updatedEntity.PriceCurrencyNavigation = navigationEntity;
			}

			if (updatedEntity.Category == entity.Category)
				updatedEntity.CategoryNavigation = entity.CategoryNavigation;
			else
			{
				var navigationEntity = _db.CategoryTrees.Find(updatedEntity.Category);
				if (navigationEntity == null)
					throw new PersistantItemNotFoundException(typeof(Entities.Tag));
				updatedEntity.CategoryNavigation = navigationEntity;
			}

			var tagEntities = model.Tags.Select(t =>
			{
				var tagEntity = _db.Tags.Find(t.GetCriterion().GetKeys());
				if (tagEntity == null) throw new PersistantItemNotFoundException(typeof(Entities.Tag));
				return tagEntity;
			});
			entity.Tags.Clear();
			foreach (var tagEntity in tagEntities)
			{
				entity.Tags.Add(tagEntity);
			}

			updatedEntity.Adapt(entity);
			await _db.SaveChangesAsync();

			return entity.Adapt<Domain.Models.MarketItem>();
		}
	}
}
