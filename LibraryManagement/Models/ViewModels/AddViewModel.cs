using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels
{
    public class AddViewModel
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage= "Title is Required")]
        public string Title { get; set; }
        [Required(ErrorMessage ="Author is Required")]
        public string Author { get; set; }
        [Required(ErrorMessage ="Description is Required")]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Available copies must be at least 1.")]
        public int AvailableCopies { get; set; }

        [Required(ErrorMessage = "Cover image URL is required.")]
 
        public IFormFile CoverImageUrl { get; set; }
    }
}
