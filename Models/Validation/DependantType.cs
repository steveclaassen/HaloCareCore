using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class DependantType
    {
        [Key]
        [Required]
        public string dependentTypeCode { get; set; }

        [Required]
        [DisplayName("Dependant type")]
        public string dependentType { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}