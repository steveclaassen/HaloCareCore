using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class CoMorbidClinicalRulesView
    {
        [DisplayName("Rule type")]
        public string ruleType { get; set; }

        [DisplayName("Rule ID")]
        public int ruleID { get; set; }

        [DisplayName("Rule")]
        public string ruleName { get; set; }

        [DisplayName("Greater")]
        public decimal greater { get; set; }

        [DisplayName("Less")]
        public decimal less { get; set; }

        [DisplayName("Pathology field")]
        public string pathologyField { get; set; }
    }
}