using pfebackend.Models;

namespace pfebackend.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category> GetCategoryById(int id);
        Task<bool> CategoryExistsAsync(string categoryName);
    }
}