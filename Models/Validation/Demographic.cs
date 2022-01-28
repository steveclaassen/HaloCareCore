using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class Demographic
    {
        [Key]
        [Required]
        public string demographicCode { get; set; }

        [DisplayName("Demographic")]
        public string demographicName { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}