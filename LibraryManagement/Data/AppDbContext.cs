using LibraryManagement.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LibraryManagement.Data
{
    public class AppDbContext: IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<ReturnPolicy> ReturnPolicies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ReturnPolicy>().HasData(new ReturnPolicy
            {
                Id = Guid.Parse("c7e1be17-ac75-431f-a85a-2db5ae92dd37"),
                ReturnDurationDays = 14
            });

        }


    }
}
