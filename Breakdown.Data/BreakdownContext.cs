using Breakdown.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Breakdown.Data
{
    public class BreakdownContext: DbContext
    {
        public BreakdownContext()
        {
        }

        public BreakdownContext(DbContextOptions<BreakdownContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.OuterId })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }
    }
}
