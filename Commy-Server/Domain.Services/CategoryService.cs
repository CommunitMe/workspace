using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class CategoryService : ICategoryService, ISearchProvider
    {
        private readonly ILogger<CategoryService> logger;
        private readonly IDatabaseTransaction db;

        public CategoryService(ILogger<CategoryService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<List<Category>?> GetAll()
        {
            IEnumerable<Category> result = await db.Categories.GetAll();

            return result.ToList();
        }

        public async Task<Category?> GetCategoryById(long id)
        {
            var category = await db.Categories.GetById(new CategoryCriterion { Id = id });

            if (category == null)
            {
                this.logger.LogInformation(message: "No Category item with id '{0}'", id);
            }

            return category;
        }

        public async Task<Category> CreateCategory(Category CategoryData)
        {
            CategoryData = await db.Categories.Create(CategoryData);
            await db.SaveChangesAsync();
            return CategoryData;
        }

        public async Task<Category> UpdateCategory(Category CategoryData)
        {
            CategoryData = await db.Categories.Update(CategoryData);
            await db.SaveChangesAsync();
            return CategoryData;
        }

        public async Task DeleteCategory(Category CategoryData)
        {
            await db.Categories.Delete(CategoryData);
            await db.SaveChangesAsync();
        }

        public async Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys)
        {
            if (scKeys == null)
            {
                return null;
            }

            List<string> servicesKeys = scKeys.Split(",").ToList();
            var categories = (await GetAll())?
                .Where(p => servicesKeys.Contains(p.Name))
                .ToList() ?? new List<Category>();

            if (categories.Count == 0)
            {
                return null;
            }

            return new SearchResult
            {
                EntityTypeId = EntityType.Categories.ID,
                EntityIds = categories.Select(m => m.Id).ToList()
            };
        }

        public EntityType GetSearchResultType()
        {
            return EntityType.Categories;
        }
    }
}
