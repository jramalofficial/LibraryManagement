namespace LibraryManagement.Models.Entities
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }

        public bool IsAvailable { get; set; }
        public int AvailableCopies { get; set; }
    }
}
