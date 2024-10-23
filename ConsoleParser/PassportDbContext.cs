using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{
    public class PassportDbContext : DbContext
    {
        public PassportDbContext(DbContextOptions<PassportDbContext> options) : base(options) { }
        public DbSet<Passport> Passports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Passport>().ToTable("passports");
            modelBuilder.Entity<Passport>().HasKey(p => p.Id);
            modelBuilder.Entity<Passport>().Property(p => p.PASSP_SERIES).IsRequired();
            modelBuilder.Entity<Passport>().Property(p => p.PASSP_NUMBER).IsRequired();
        }
    }
}
