using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels
{
    public class ReturnPolicyViewModal
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Return policy name is required.")]
        public int ReturnDurationDays { get; set; }
    }
}
