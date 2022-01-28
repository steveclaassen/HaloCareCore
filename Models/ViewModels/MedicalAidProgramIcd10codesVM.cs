using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    //HCare-1447
    public class MedicalAidProgramIcd10codesVM
    {
        public Guid Id { get; set; }
        public Guid MedicalAidId { get; set; }
        [DisplayName("Medical aid")]
        public string MedicalAidName { get; set; }
        [DisplayName("Program")]
        public string Program { get; set; }
        [DisplayName("Icd10-code")]
        public string icd10Code { get; set; }
        public string createdBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Checked")]
        public bool check { get; set; }
    }
}