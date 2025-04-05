using Microsoft.EntityFrameworkCore;
using pfebackend.Models;

namespace pfebackend.Data
{
    public static class CategorySeeder
    {
        public static void SeedCategories(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Transport" },
                new Category { Id = 3, Name = "Entertainment" },
                new Category { Id = 4, Name = "Health" },
                new Category { Id = 5, Name = "Electronics" },
                new Category { Id = 6, Name = "Fashion" },
                new Category { Id = 7, Name = "Housing" },
                new Category { Id = 8, Name = "Others" }
            );
        }
    }
}