using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class BenefitType
    {
        [Key]
        [Required]
        [DisplayName("Benefit type")]
        public string benefitType { get; set; }

        [Required]
        [DisplayName("Name")]
        public string benefitTypeName { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}