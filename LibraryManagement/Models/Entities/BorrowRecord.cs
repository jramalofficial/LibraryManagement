namespace LibraryManagement.Models.Entities
{
    public class BorrowRecord
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }           
        public string UserId { get; set; }       
        public DateTime BorrowDate { get; set; }

        public Book Book { get; set; }
    }
}
