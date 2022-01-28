using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    //HCare-1184
    public class HIVStagingRiskRules
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Stage")]
        public string stage { get; set; }

        [DisplayName("Risk rating")]
        public string RiskId { get; set; }

        [Required]
        [DisplayName("TB")]
        public bool TB { get; set; }

        [Required]
        [DisplayName("Cancer")]
        public bool cancer { get; set; }

        [Required]
        [DisplayName("CDL")]
        public bool cdl { get; set; }

        [DisplayName("Renal failure")]
        public bool renal { get; set; }

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