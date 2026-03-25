using Domain.Abstractions.Common;
using Domain.Infra.Exceptions;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CmeDbContext _db;

        public CategoryRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Category> Create(Category model)
        {
            var data = model.Adapt<Entities.CategoryTree>();

            var result = _db.CategoryTrees.Add(data);
            await _db.SaveChangesAsync();

            return result.Entity.Adapt<Domain.Models.Category>();
        }

        public async Task Delete(Category model)
        {
            ICriterion<Category>? criterion = model.GetCriterion();
            var row = await _db.CategoryTrees.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(Category).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(_db.CategoryTrees.Entry(row));
            await _db.SaveChangesAsync();
        }

        public Task<List<Category>> GetAll(ICriterion<Category>? criterion)
        {
            var query = _db.CategoryTrees.Select(m => m.Adapt<Domain.Models.Category>());

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<Category?> GetById(ICriterion<Category> criterion)
        {
            var result = await _db.CategoryTrees.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            return result.Adapt<Domain.Models.Category>();
        }

        public async Task<Category> Update(Category model)
        {
            var entity = await _db.CategoryTrees.FindAsync(model.GetCriterion().GetKeys());

            if (entity == null)
                throw new KeyNotFoundException($"Failed to find {nameof(Category)} with given criterion");

            var updatedEntity = model.Adapt<Entities.CategoryTree>();

            if (updatedEntity.Parent == entity.Parent)
                updatedEntity.ParentNavigation = entity.ParentNavigation;
            else
            {
                var navigationEntity = _db.CategoryTrees.Find(updatedEntity.Parent);
                if (navigationEntity == null)
                    throw new PersistantItemNotFoundException(typeof(Category));
                updatedEntity.ParentNavigation = navigationEntity;
            }

            updatedEntity.Adapt(entity);
            await _db.SaveChangesAsync();
            return entity.Adapt<Domain.Models.Category>();
        }
    }
}
