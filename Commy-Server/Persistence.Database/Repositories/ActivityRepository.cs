using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Persistence.Database.ModelAdapters;

namespace Persistence.Database.Repositories
{
	public class ActivityRepository : IActivityRespository
	{
		private readonly CmeDbContext _db;

		public ActivityRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public Task<Activity> Create(Activity model)
		{
			throw new NotImplementedException();
		}

		public async Task Delete(Activity model)
		{
			ICriterion<Activity>? criterion = model.GetCriterion();
			var row = await _db.Activities.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(Activity).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Remove(_db.Activities.Entry(row));
			await _db.SaveChangesAsync();
		}

		public Task<List<Activity>> GetAll(ICriterion<Activity>? criterion)
		{
			var query = _db.Activities.ToModel();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Activity?> GetById(ICriterion<Activity> criterion)
		{
			var result = await _db.Activities.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			return result.ToModel(_db);
		}

		public void SaveChanges()
		{
			_db.SaveChanges();
		}

		public Task<Activity> Update(Activity model)
		{
			throw new NotImplementedException();
		}
	}
}
