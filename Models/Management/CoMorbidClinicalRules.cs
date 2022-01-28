using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class CoMorbidClinicalRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string ruleName { get; set; }

        [DisplayName("Higher value")]
        public decimal greater { get; set; }

        [DisplayName("Lesser value")]
        public decimal less { get; set; }

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

        public bool active { get; set; }

        [DisplayName("Pathology field")]
        public string pathologyField { get; set; }
    }
}