using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Database.Repositories
{
    public class CommunitySettingsRepository : ICommunitySettingsRepository
    {
        private readonly CmeDbContext _db;

        public CommunitySettingsRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CommunitySettings> Create(CommunitySettings model)
        {
            var data = model.Adapt<Entities.CommunitySetting>();

            var result = _db.CommunitySettings.Add(data);
            await _db.SaveChangesAsync();

            return result.Entity.Adapt<CommunitySettings>();
        }

        public async Task Delete(CommunitySettings model)
        {
            ICriterion<CommunitySettings>? criterion = model.GetCriterion();
            var row = await _db.CommunitySettings.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(CommunitySettings).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(_db.CommunitySettings.Entry(row));
            await _db.SaveChangesAsync();
        }

        public Task<List<CommunitySettings>> GetAll(ICriterion<CommunitySettings>? criterion)
        {
            var query = _db.CommunitySettings.Select(m => m.Adapt<CommunitySettings>());

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<CommunitySettings?> GetById(ICriterion<CommunitySettings> criterion)
        {
            var result = await _db.CommunitySettings.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            return result.Adapt<CommunitySettings>();
        }

        public async Task<CommunitySettings?> GetByCommunityId(long id)
        {
            var result = await _db.CommunitySettings.FirstOrDefaultAsync(x => x.CommunityId == id);

            if (result == null)
                return null;

            return result.Adapt<CommunitySettings>();
        }

        public async Task<CommunitySettings> Update(CommunitySettings model)
        {
            ICriterion<CommunitySettings>? criterion = model.GetCriterion();
            var row = await _db.CommunitySettings.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(CommunitySettings).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            var data = model.Adapt(row);
            _db.CommunitySettings.Update(data);
            await _db.SaveChangesAsync();

            return data.Adapt<CommunitySettings>();
        }
    }
}