using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class AssignmentTypes
    {
        [Key]
        [Required]
        [DisplayName("Code")]
        public string assignmentType { get; set; }

        [Required]
        [DisplayName("Description")]
        public string assignmentDescription { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}