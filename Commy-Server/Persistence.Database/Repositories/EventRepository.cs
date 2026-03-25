using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ExpressionDebugger;
using Domain.Models.Criteria;
using Domain.Infra.Exceptions;

namespace Persistence.Database.Repositories
{
	public class EventRepository : IEventRepository
	{
		private readonly CmeDbContext _db;

		public EventRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<Event> Create(Event model)
		{
			var data = model.Adapt<Entities.Event>();

			var result = _db.Events.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.Event>();
		}

		public async Task Delete(Event model)
		{
			ICriterion<Event>? criterion = model.GetCriterion();
			var row = await _db.Events.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(Event).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			await _db.Entry(row).Collection(e => e.Profiles).LoadAsync();

			row.Profiles.Clear();

			_db.Events.Remove(row);
			await _db.SaveChangesAsync();
		}

		public Task<List<Event>> GetAll(ICriterion<Event>? criterion)
		{
			var query = _db.Events.Include(m => m.Profiles)
												  .Select(m => m.Adapt<Domain.Models.Event>())
												  .AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(criterion.IsMet);
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Event?> GetById(ICriterion<Event> criterion)
		{
			var result = await _db.Events.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			_db.Entry(result).Reference(m => m.RelevantCommunityNavigation).Load();
			_db.Entry(result).Collection(m => m.Profiles).Load();

			return result.Adapt<Domain.Models.Event>();
		}

		public async Task<Event> Update(Event model)
		{
			var result = await _db.Events.FindAsync(model.GetCriterion().GetKeys());

			if (result == null)
				throw new KeyNotFoundException($"Failed to find {nameof(Event)} with given criterion");

			await _db.Entry(result).Collection(e => e.Profiles).LoadAsync();

			result.Location = model.Location;
			result.EventTime = model.EventTime;
			result.Name = model.Name;
			result.ImageUid = model.ImageUid;
			result.RelevantCommunity = model.RelevantCommunity;

			HashSet<Entities.Profile> existingProfiles = new HashSet<Entities.Profile>();
			HashSet<Entities.Profile> removedProfiles = new HashSet<Entities.Profile>();

			// Gather what profiles need to be removed
			foreach(var profile in result.Profiles)
			{
				if (model.Interested.Any(pid => profile.Id == pid))
				{
					existingProfiles.Add(profile);
				}
				else
				{
					removedProfiles.Add(profile);
				}
			}

			// Remove said profiles
			foreach (var profile in removedProfiles)
			{
				result.Profiles.Remove(profile);
			}
		
			// Add new profiles
			foreach (var profileId in model.Interested)
			{
				Entities.Profile? profile = await _db.Profiles.FindAsync(new object[] { profileId });

				if (profile == null)
				{
					throw new PersistantItemNotFoundException(typeof(Entities.Profile));
				}

				if (!existingProfiles.Contains(profile))
				{
					result.Profiles.Add(profile);
				}
			}
			await _db.SaveChangesAsync();

			return result.Adapt<Domain.Models.Event>();
		}
	}
}
