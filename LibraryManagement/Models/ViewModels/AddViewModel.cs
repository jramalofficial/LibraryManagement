using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels
{
    public class AddViewModel
    {
        [Required(ErrorMessage= "Title is Required")]
        public string Title { get; set; }
        [Required(ErrorMessage ="Author is Required")]
        public string Author { get; set; }
        [Required(ErrorMessage ="Description is Required")]
        public string Description { get; set; }
    }
}
