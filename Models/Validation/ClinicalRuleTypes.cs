using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ClinicalRuleTypes
    {
        [Key]
        [Required]
        [DisplayName("Rule type")]
        public string ruleType { get; set; }

        [DisplayName("Rule")]
        public string ruleName { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}