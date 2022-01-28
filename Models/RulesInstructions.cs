using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class RulesInstructions
    {
        [DisplayName("Rule name")]
        public string ruleName { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Message")]
        public List<ClinicalMessageInstructions> messages { get; set; }

        public string program { get; set; }

        [DisplayName("Rule broken")]
        public string RuleBroken { get; set; } //hcare-1344

        [DisplayName("Instructions")]
        public string RuleInstructions { get; set; } //hcare-1344
    }
}