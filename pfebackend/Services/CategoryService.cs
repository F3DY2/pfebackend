using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(string categoryName)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == categoryName.Trim().ToLower());
        }
    }
}