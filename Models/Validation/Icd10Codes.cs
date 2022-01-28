using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class Icd10Codes
    {
        public Guid Id { get; set; } //hcare-1280

        [Key]
        [DisplayName("ICD-10 code")]
        public string icd10CodeID { get; set; }

        [DisplayName("Code type")]
        public string codeType { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Created by")]
        public string modifiedBy { get; set; } //hcare-1280

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; } //hcare-1280

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("SubCode")]
        public string subcode { get; set; }
    }
}