using Loan.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Loan.Data
{
    public class FinalProjectDbContext : IdentityDbContext<Core.Entities.User>
    {
        public FinalProjectDbContext(DbContextOptions<FinalProjectDbContext> options) : base(options) { }

        public DbSet<Core.Entities.Loan> Loan { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
