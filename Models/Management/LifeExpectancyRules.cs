using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class LifeExpectancyRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [Required]
        [DisplayName("Gender")]
        public string gender { get; set; }

        [DisplayName("Risk rating")]
        public string RiskId { get; set; }

        [Required]
        [DisplayName("Smoker")]
        public bool smoker { get; set; }

        [Required]
        [DisplayName("Hypertension")]
        public bool hypertension { get; set; }

        [Required]
        [DisplayName("HYCHOL")]
        public bool hychol { get; set; }

        [DisplayName("> HbA1c")]
        public decimal gtHba1C { get; set; }

        [DisplayName("< HbA1c")]
        public decimal ltHba1C { get; set; }

        [DisplayName("> Age")]
        public decimal gtAge { get; set; }

        [DisplayName("< Age")]
        public decimal ltAge { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }
        
        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}