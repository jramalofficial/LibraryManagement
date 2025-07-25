using LibraryManagement.Models.Entities; 
using Microsoft.AspNetCore.Identity;
namespace LibraryManagement.Models.Entities
{
    public class BorrowRecord
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }           
        public string UserId { get; set; }       
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnDate { get; set; } 
        public bool IsReturned { get; set; } = false;
        public Book Book { get; set; }
       
    }
}
