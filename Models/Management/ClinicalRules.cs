using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class ClinicalRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
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

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Pathology field")]
        public string pathologyField { get; set; }

        [DisplayName("Assignment")]
        public bool hasAssignment { get; set; }

        [DisplayName("Assignment name")]
        public string assignmentItem { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; } //hcare-1344

        public List<AssignmentItemTypes> Assignments { get; set; }
        public List<PathologyFields> Pathologies { get; set; }
        public List<Programs> Programs { get; set; }


    }
}