using Microsoft.EntityFrameworkCore;
using URL_Shortener.Models.Entities;

namespace URL_Shortener.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Url> Urls { get; set; }
    }
}
