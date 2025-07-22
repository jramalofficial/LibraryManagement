using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Role { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
