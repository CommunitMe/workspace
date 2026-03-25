using Domain.Abstractions.Common;
using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
	public class CommunityRepository : ICommunityRepository
	{
		private readonly CmeDbContext _db;

		public CommunityRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<Community> Create(Community model)
		{
			var data = model.Adapt<Entities.Community>();

			var result = _db.Communities.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.Community>();
		}

		public async Task Delete(Community model)
		{
			ICriterion<Community>? criterion = model.GetCriterion();
			var row = await _db.Communities.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(Community).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Remove(_db.Communities.Entry(row));
			await _db.SaveChangesAsync();
		}

		public Task<List<Community>> GetAll(ICriterion<Community>? criterion)
		{
			var query = _db.Communities.Select(m => m.Adapt<Domain.Models.Community>());

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<Community?> GetById(ICriterion<Community> criterion)
		{
			var result = await _db.Communities.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			return result.Adapt<Domain.Models.Community>();
		}

		public async Task<List<Community>> GetCommunitiesByProfile(ICriterion<Profile> criterion)
		{
			var result = await _db.Profiles.FindAsync(criterion.GetKeys());

			if (result == null)
			{
				throw new PersistantItemNotFoundException(typeof(Entities.Profile));
			}

			await _db.Entry(result).Collection(reference => reference.CommunityProfiles).LoadAsync();

            foreach (var communityProfile in result.CommunityProfiles)
            {
                await _db.Entry(communityProfile).Reference(reference => reference.Community).LoadAsync();
            }

			return result.CommunityProfiles.Select(c => c.Community).Adapt<List<Domain.Models.Community>>();
		}

		public async Task AssignProfileToCommunity(ICriterion<Profile> profileCriterion, ICriterion<Community> communityCriterion)
		{
			var community = await _db.Communities.FindAsync(communityCriterion.GetKeys());

			if (community == null)
				throw new KeyNotFoundException("Failed to find community with given criterion");

			var profile = await _db.Profiles.FindAsync(profileCriterion.GetKeys());

			if (profile == null)
				throw new KeyNotFoundException("Failed to find profile with given criterion");

			community.Profiles.Add(profile);
			await _db.SaveChangesAsync();
		}

		public async Task<bool> RemoveProfileFromCommunity(ICriterion<Profile> profileCriterion, ICriterion<Community> communityCriterion)
		{
			var community = await _db.Communities.FindAsync(communityCriterion.GetKeys());

			if (community == null)
				throw new KeyNotFoundException("Failed to find community with given criterion");

			var profile = await _db.Profiles.FindAsync(profileCriterion.GetKeys());

			if (profile == null)
				throw new KeyNotFoundException("Failed to find profile with given criterion");

			if (!community.Profiles.Remove(profile)) 
			{
				return false;
			}
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<Community> Update(Community model)
		{
			var result = await _db.Communities.FindAsync(model.GetCriterion().GetKeys());

			if (result == null)
				throw new KeyNotFoundException("Failed to find community with given criterion");

			result.Description = model.Description;
			await _db.SaveChangesAsync();

			return result.Adapt<Domain.Models.Community>();
		}
	}
}
