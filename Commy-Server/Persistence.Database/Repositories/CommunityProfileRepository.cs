using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
    public class CommunityProfileRepository : ICommunityProfileRepository
    {
        private readonly CmeDbContext _db;
        public CommunityProfileRepository(CmeDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<CommunityProfile> Create(CommunityProfile model)
        {
            var data = model.Adapt<Entities.CommunityProfile>();

            var result = _db.CommunityProfiles.Add(data);
            await _db.SaveChangesAsync();

            return result.Entity.Adapt<Domain.Models.CommunityProfile>();
        }

        public async Task Delete(CommunityProfile model)
        {
            ICriterion<CommunityProfile>? criterion = model.GetCriterion();
            var row = await _db.CommunityProfiles.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(CommunityProfile).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(_db.CommunityProfiles.Entry(row));
            await _db.SaveChangesAsync();
        }

        public Task<List<CommunityProfile>> GetAll(ICriterion<CommunityProfile>? criterion = null)
        {
            var query = _db.CommunityProfiles.Select(m => m.Adapt<Domain.Models.CommunityProfile>());

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<CommunityProfile?> GetById(ICriterion<CommunityProfile> criterion)
        {
            var result = await _db.CommunityProfiles.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            await _db.Entry(result).Reference(reference => reference.Community).LoadAsync();
            await _db.Entry(result).Reference(reference => reference.Profile).LoadAsync();

            return result.Adapt<Domain.Models.CommunityProfile>();
        }

        public async Task<CommunityProfile> Update(CommunityProfile model)
        {
            var entity = await _db.CommunityProfiles.FindAsync(model.GetCriterion().GetKeys());

            if (entity == null)
                throw new NullReferenceException($"Failed to find {nameof(CommunityProfile)} with given criterion");

            entity.MemberState = model.MemberState;
            entity.ProviderState = model.ProviderState;
            await _db.SaveChangesAsync();

            return entity.Adapt<Domain.Models.CommunityProfile>();
        }
    }
}