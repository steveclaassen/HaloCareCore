using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ScriptViewModel
    {
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required]
        [DisplayName("Repeats")]
        public int repeats { get; set; }

        [Required]
        [DisplayName("Product name")]
        public string productName { get; set; }

        [Required]
        [DisplayName("NAPPI")]
        public string nappiCode { get; set; }

        [Required]
        [DisplayName("Authorisation outcome")]
        public string directions { get; set; }

        [Required]
        [DisplayName("Qty")]
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

        [DisplayName("Item repeats")]
        public int itemRepeats { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Item status")]
        public string itemStatus { get; set; }

        [DisplayName("Strength")]
        public string strength { get; set; }

        [DisplayName("Item #")]
        public int itemNo { get; set; }

        [DisplayName("Benefit type")]
        public string benefitType { get; set; }

        [DisplayName("Script ID")]
        public string scriptID { get; set; }

        [DisplayName("Script type")]
        public string scriptType { get; set; }

        [DisplayName("Prescribing Dr.")]
        public string prescribingDr { get; set; }

        public bool authItem { get; set; }

        [DisplayName("ICD-10")]
        public string icd10code { get; set; }

        [DisplayName("Code type")]
        public string icd10Type { get; set; }

        [DisplayName("Prophylaxis")]
        public bool prophylaxis { get; set; }

        [DisplayName("Line note")]
        public string comment { get; set; }

        public string createdBy { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
        
        [DisplayName("CL item #")]
        public string CLItemLineNo { get; set; }

        public string program { get; set; }

        #region HCare-105 Produts
        [DisplayName("ATC Code")]
        public string atcCode { get; set; }
        [DisplayName("MIMS Class description")]
        public string therapeuticClass6Desc { set; get; }

        [DisplayName("MIMS Class number")]
        public int therapeuticClass6 { get; set; }

        [DisplayName("MMAP Indicator")]
        public string mmapIndicator { set; get; }

        [DisplayName("MRP Indicator")]
        public string mrpIndicator { get; set; }

        [DisplayName("Pack unit of measure")]
        public string packUOM { get; set; }

        [DisplayName("Generic Indicator ")]
        public string genericIndicator { get; set; }

        [DisplayName("Medicine Schedule")]
        public string productSchedule { set; get; }

        [DisplayName("product metric strength")]
        public string strengthUOM { get; set; }
        #endregion


    }
}