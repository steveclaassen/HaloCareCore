using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class CoMorbidTypeRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Rule type")]
        public string ruleType { get; set; }

        [Required]
        [DisplayName("Rule ID")]
        public int ruleID { get; set; }
    }
}