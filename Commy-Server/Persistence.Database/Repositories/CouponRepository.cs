using Domain.Abstractions.Common;
using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Database.Repositories
{
	public class CouponRepository : ICouponRepository
	{
		private readonly CmeDbContext _db;

		public CouponRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<Coupon> Create(Coupon model)
		{
			var data = model.Adapt<Entities.Coupon>();

			var result = _db.Coupons.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.Coupon>();
		}

		public async Task Delete(Coupon model)
		{
			ICriterion<Coupon>? criterion = model.GetCriterion();
			var row = await _db.Coupons.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(Coupon).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Remove(row);
			await _db.SaveChangesAsync();
		}

		public Task<List<Coupon>> GetAll(ICriterion<Coupon>? criterion)
		{
			var query = _db.Coupons.Include(m => m.RelevantCommunityNavigation)
						.Include(m => m.Tags)
						.Select(e=>e.Adapt<Domain.Models.Coupon>()).AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Coupon?> GetById(ICriterion<Coupon> criterion)
		{
			var entity = await _db.Coupons.FindAsync(criterion.GetKeys());

			if (entity == null)
				return null;

			_db.Entry(entity).Reference(m => m.RelevantCommunityNavigation).Load();
			_db.Entry(entity).Collection(m => m.Tags).Load();

			return entity.Adapt<Domain.Models.Coupon>();
		}


		public async Task<Coupon> Update(Coupon model)
		{
			var entity = await _db.Coupons.FindAsync(model.GetCriterion().GetKeys());

			if (entity == null)
				throw new KeyNotFoundException($"Failed to find {nameof(Coupon)} with given criterion");

			var updatedEntity = model.Adapt<Entities.Coupon>();

			if (updatedEntity.RelevantCommunity == entity.RelevantCommunity)
				updatedEntity.RelevantCommunityNavigation = entity.RelevantCommunityNavigation;
			else
			{
				var navigationEntity = _db.Communities.Find(updatedEntity.RelevantCommunity);
				if (navigationEntity == null)
					throw new PersistantItemNotFoundException(typeof(Community));
				updatedEntity.RelevantCommunityNavigation = navigationEntity;
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

			return entity.Adapt<Domain.Models.Coupon>();
		}
	}
}
