using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Script
{
    public class ScriptItems
    {
        [Key]
        [Required]
        public int itemNo { get; set; }

        [Required]
        [DisplayName("Script ID")]
        public int scriptID { get; set; }

        [Required]
        [DisplayName("NAPPI")]
        public string nappiCode { get; set; }

        [Required]
        [DisplayName("Directions")]
        public string directions { get; set; }

        [Required]
        [DisplayName("Quantity")]
        public string quantity { get; set; }

        [Required]
        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime fromDate { get; set; }

        [Required]
        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? toDate { get; set; }

        [DisplayName("Item Repeats")]
        public int itemRepeats { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
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

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Item status")]
        public string itemStatus { get; set; }

        [DisplayName("Prophylaxis")]
        public bool prophylaxis { get; set; }

        [DisplayName("Item type")]
        public string itemType { get; set; }

        [DisplayName("Benefit type")]
        public string benefitType { get; set; }

        [DisplayName("Co-Payment override")]
        public bool coPaymentOverride { get; set; }

        [DisplayName("ICD-10 code")]
        public string icd10code { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("CL item #")]
        public string CLItemLineNo { get; set; }

    }
}