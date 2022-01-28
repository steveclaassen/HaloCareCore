using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class HospitalizationAuthEvents
    {
        [Key]
        [Required]
        public int id { get; set; }

        public int refNo { get; set; }

        [DisplayName("Epi auth")]
        public string epiAuth { get; set; }

        [DisplayName("Event code")]
        public string eventCode { get; set; }

        [DisplayName("Diagnosis")]
        public string diagnosis { get; set; }

        [DisplayName("Associated diagnosis")]
        public string associatedDiagnosis { get; set; }

        [DisplayName("Procedure code")]
        public string procedureCode { get; set; }

        [DisplayName("Modified by")]
        public string procedureModifier { get; set; }

        [DisplayName("Main")]
        public string main { get; set; }

        [DisplayName("Minimum benefit")]
        public string minimunBenefitCode { get; set; }

        [DisplayName("PMB applicable")]
        public string pmbApplicable { get; set; }

        [DisplayName("CDL")]
        public string cdlCode { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createddate { get; set; }

        [DisplayName("AdditionalDiagnosis")]
        public string additionalDiagnosis { get; set; }

        [DisplayName("finalDiagnosis")]
        public string finalDiagnosis { get; set; }
    }
}