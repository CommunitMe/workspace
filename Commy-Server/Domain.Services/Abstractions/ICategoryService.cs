using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface ICategoryService
    {
        Task<List<Category>?> GetAll();
        Task<Category?> GetCategoryById(long id);
        Task<Category> CreateCategory(Category categoryData);
        Task<Category> UpdateCategory(Category categoryData);
        Task DeleteCategory(Category categoryData);
    }
}
