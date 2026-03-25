using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Persistence.Database.ModelAdapters;

namespace Persistence.Database.Repositories
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly CmeDbContext _db;

		public NotificationRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<Notification> Create(Notification model)
		{
			var data = model.Adapt<Entities.Notification>();

			var result = _db.Notifications.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.Notification>();
		}

		public async Task Delete(Notification model)
		{
			throw new NotImplementedException();
		}

		public Task<List<Notification>> GetAll(ICriterion<Notification>? criterion)
		{
			var query = _db.Notifications.Include(m => m.RelevantCommunityNavigation)
										   .Select(m => m.Adapt<Domain.Models.Notification>())
										   .AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Notification?> GetById(ICriterion<Notification> criterion)
		{
			throw new NotImplementedException();
		}

		public Task<Notification> Update(Notification model)
		{
			throw new NotImplementedException();
		}
	}
}
