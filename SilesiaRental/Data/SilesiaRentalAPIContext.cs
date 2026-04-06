using Microsoft.EntityFrameworkCore;
using SilesiaRental.Models;
using System.ComponentModel.DataAnnotations;

namespace SilesiaRental.Data {
    public class SilesiaRentalAPIContext : DbContext {
        public SilesiaRentalAPIContext(DbContextOptions<SilesiaRentalAPIContext> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tool>()
                .Property(t => t.PricePerDay)
                .HasColumnType("decimal(18,2)");
        }

        [ConcurrencyCheck]
        public Tool Version { get; set; }

        public DbSet<Tool> Tools { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
