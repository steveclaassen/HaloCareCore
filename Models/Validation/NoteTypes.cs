using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class NoteTypes
    {
        [Key]
        [Required]
        [DisplayName("Note type")]
        public string noteType { get; set; }

        [Required]
        [DisplayName("Description")]
        public string noteDescription { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}