using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class Sex
    {
        [Key]
        [Required]
        public string sex { get; set; }

        [DisplayName("Gender")]
        public string sexName { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}