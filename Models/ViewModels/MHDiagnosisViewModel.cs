using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class MHDiagnosisViewModel
    {
        public int Id { get; set; }

        public Guid DependantID { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("ICD-10 code")] 
        public string ICD10Code { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; } 

        [DisplayName("Mapping code")]
        public string MappingCode { get; set; }
        public string MappingDescription { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public PatientProgramSubHistory PatientProgramSubHistory { get; set; }
        public List<CoMorbidConditionVM> ConditionList { get; set; }
        public List<CoMorbidConditionVM> ICD10List { get; set; }
    }
}