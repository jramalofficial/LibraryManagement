using LibraryManagement.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data
{
    public class AppDbContext: IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }  


    }
}
