using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class Programs
    {
        [Key]
        public Guid programID { get; set; }

        [Required]
        [DisplayName("Program")]
        public string ProgramName { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

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

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Code")]
        [StringLength(10, ErrorMessage = "Code restricted to 10 characters")] //HCare-969
        public string code { get; set; }

        [DisplayName("ICD-10 code")] //HCare-958
        public string icd10code { get; set; }

        [DisplayName("Allow Multiples")] //HCare-1098
        public bool multipleICD10 { get; set; }
    }
}