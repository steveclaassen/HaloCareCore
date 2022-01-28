using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class RiskRatingTypes
    {
        [Key]
        [Required]
        public string RiskType { get; set; }

        [DisplayName("Risk Name")]
        public string RiskName { get; set; }

        [DisplayName("Risk Priority")]
        public string RiskPriority { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}