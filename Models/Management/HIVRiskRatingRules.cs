using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class HIVRiskRatingRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [DisplayName("CD4 count")]
        public bool CD4Count { get; set; }

        [DisplayName("CD4 percentage")]
        public bool CD4Percentage { get; set; }

        [DisplayName("Viral load")]
        public bool viralLoad { get; set; }

        [DisplayName("lt Value")]
        public int ltValue { get; set; }

        [DisplayName("gt Value")]
        public int gtValue { get; set; }

        [DisplayName("Stage")]
        public int stage { get; set; }

        [Required]
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