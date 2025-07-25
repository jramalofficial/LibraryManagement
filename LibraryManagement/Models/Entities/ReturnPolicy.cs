using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Entities
{
    public class ReturnPolicy
    {
        public Guid Id { get; set; }
      
        public int ReturnDurationDays { get; set; }
    }
}
