using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class QueryTypes
    {
        [Key]
        [Required]
        [DisplayName("Query type")]
        public string code { get; set; }

        [Required]
        [DisplayName("Query")]
        public string queryDescription { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}