using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class ICD10OnDischargeSetup
    {
        [key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Program")]
        public string Program { get; set; }
        [NotMapped]
        public List<Programs> Programs { get; set; }
        [NotMapped]
        public List<ICD10Master> iCD10s { get; set; }

        [Required]
        [DisplayName("ICD-10 on discharge")]
        public string ICD10_on_discharge { get; set; }

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