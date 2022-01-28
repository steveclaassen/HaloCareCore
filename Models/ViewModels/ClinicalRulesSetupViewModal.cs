using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class ClinicalRulesSetupViewModal
    {
        public Guid planruleID { get; set; }
        public Guid accountId { get; set; }
        public Guid programId { get; set; }
        public Guid mediaidId { get; set; }

        public bool Added { get; set; }
        public int id { get; set; }

        [DisplayName("Name")]
        public string ruleName { get; set; }

        [DisplayName("Higher value")]
        public decimal greater { get; set; }

        [DisplayName("Higher message")]
        public string gtMessage { get; set; }

        [DisplayName("Lesser value")]
        public decimal less { get; set; }

        [DisplayName("Lesser message")]
        public string ltMessage { get; set; }

        [DisplayName("Clinical rule type")]
        public string ruleType { get; set; }

        [DisplayName("Male")]
        public bool male { get; set; }

        [DisplayName("Female")]
        public bool female { get; set; }

        [DisplayName("Pathology field")]
        public string pathologyField { get; set; }
    }
}