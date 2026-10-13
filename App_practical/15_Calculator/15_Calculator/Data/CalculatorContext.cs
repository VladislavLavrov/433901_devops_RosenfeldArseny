using _15_Calculator.Models;
using Microsoft.EntityFrameworkCore;

namespace _15_Calculator.Data
{
    public class CalculatorContext : DbContext
    {
        public DbSet<DataInputVariant> DataInputVariants { get; set; }

        public CalculatorContext(DbContextOptions<CalculatorContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ничего дополнительно не нужно
        }
    }
}