using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class MedicalAidProgramIcd10codes //HCare-1447
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid MedicalAidID { get; set; }

        [Required]
        [DisplayName("Program code")]
        public string ProgramCode { get; set; }

        [Required]
        [DisplayName("ICD-10 code")]
        public string icd10Code { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}