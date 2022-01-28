using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class HypoglymiaRiskHistory
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
        public Guid dependantID { get; set; }

        [DisplayName("Hypo Risk")]
        public bool HypoRisk { get; set; }
        [DisplayName("Insulin")]
        public bool Insulin { get; set; }
        [DisplayName("Sulphonylureas")]
        public bool Sulphonylureas { get; set; }
        [DisplayName("Glucose")]
        public string Glucose { get; set; }
        [DisplayName("Renalfailure")]
        public bool Renal { get; set; }
        [DisplayName("HypoHospitalization")]
        public bool hypo { get; set; }
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