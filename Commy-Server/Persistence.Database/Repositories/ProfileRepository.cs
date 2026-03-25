using Domain.Abstractions.Common;
using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly CmeDbContext _db;

        public ProfileRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<Profile> Create(Profile model)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(Profile model)
        {
            ICriterion<Profile>? criterion = model.GetCriterion();
            var row = await _db.Profiles.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(Profile).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(_db.Profiles.Entry(row));
            await _db.SaveChangesAsync();
        }

        public Task<List<Profile>> GetAll(ICriterion<Profile>? criterion)
        {
            var query = _db.Profiles.Select(m => m.Adapt<Domain.Models.Profile>());

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<Profile?> GetById(ICriterion<Profile> criterion)
        {
            var result = await _db.Profiles.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            await _db.Entry(result).Collection(reference => reference.CommunityProfiles).LoadAsync();

            return result.Adapt<Domain.Models.Profile>();
        }

        public async Task<List<Profile>> GetProfilesByCommunity(ICriterion<Community> community)
        {
            var result = await _db.Communities.FindAsync(community.GetKeys());

            if (result == null)
            {
                throw new PersistantItemNotFoundException(typeof(Entities.Community));
            }

            await _db.Entry(result).Collection(reference => reference.Profiles).LoadAsync();

            return result.Profiles.Adapt<List<Domain.Models.Profile>>();
        }

        public async Task<Profile> Update(Profile model)
        {
            var entity = await _db.Profiles.FindAsync(model.GetCriterion().GetKeys());

            if (entity == null)
                throw new KeyNotFoundException($"Failed to find {nameof(Profile)} with given criterion");

            var updatedEntity = model.Adapt<Entities.Profile>();

            if (updatedEntity.DefaultCommunity == entity.DefaultCommunity)
                updatedEntity.DefaultCommunityNavigation = entity.DefaultCommunityNavigation;
            else
            {
                var navigationEntity = _db.Communities.Find(updatedEntity.DefaultCommunity);
                if (navigationEntity == null)
                    throw new PersistantItemNotFoundException(typeof(Entities.Community));
                updatedEntity.DefaultCommunityNavigation = navigationEntity;
            }

            if (updatedEntity.ServiceProvider == entity.ServiceProvider)
                updatedEntity.ServiceProviderNavigation = entity.ServiceProviderNavigation;
            else
            {
                var navigationEntity = _db.ServiceProviders.Find(updatedEntity.ServiceProvider);
                if (navigationEntity == null)
                    throw new PersistantItemNotFoundException(typeof(Entities.ServiceProvider));
                updatedEntity.ServiceProviderNavigation = navigationEntity;
            }

            updatedEntity.Adapt(entity);
            await _db.SaveChangesAsync();

            return entity.Adapt<Domain.Models.Profile>();
        }
    }
}
