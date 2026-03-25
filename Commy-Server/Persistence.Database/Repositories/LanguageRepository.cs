using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly CmeDbContext _db;

        public LanguageRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); ;
        }

        public async Task<Language> Create(Language model)
        {
            var data = model.Adapt<Entities.Language>();

            var result = _db.Languages.Add(data);
            await _db.SaveChangesAsync();

            return result.Entity.Adapt<Language>();
        }

        public async Task Delete(Language model)
        {
            ICriterion<Language>? criterion = model.GetCriterion();
            var row = await _db.Events.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{nameof(Language)}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Events.Remove(row);
            await _db.SaveChangesAsync();
        }

        public Task<List<Language>> GetAll(ICriterion<Language>? criterion = null)
        {
            var query = _db.Languages.Select(m => m.Adapt<Language>()).AsEnumerable();

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<Language?> GetById(ICriterion<Language> criterion)
        {
            var result = await _db.Languages.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            return result.Adapt<Language>();
        }

        public async Task<Language> Update(Language model)
        {
            var dbEntity = await _db.Languages.FindAsync(model.GetCriterion().GetKeys());

            if (dbEntity == null)
                throw new KeyNotFoundException($"Failed to find {nameof(Language)} with given criterion");

            model.Adapt<Entities.Language>().Adapt(dbEntity);
            await _db.SaveChangesAsync();

            return dbEntity.Adapt<Language>();
        }
    }
}
