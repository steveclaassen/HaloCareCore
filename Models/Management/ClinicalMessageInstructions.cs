using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class ClinicalMessageInstructions
    {
        [Key]
        [Required]
        public int id { get; set; }

        [DisplayName("Rule")]
        public string ruleMessage { get; set; }

        [DisplayName("Instruction")]
        public string instruction { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}