using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class Pathology
    {
        [Key]
        [Required]
        [DisplayName("ID")]
        public int pathologyID { get; set; }

        [Required]
        public Guid dependentID { get; set; }

        public Guid? questionnaireID { get; set; }

        //[Required]
        [DisplayName("Path date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required(ErrorMessage = "Select Pathology Type!")]
        [DisplayName("Type")]
        public string pathologyType { get; set; }

        [DisplayName("CD4 count")]
        public decimal? CD4Count { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CD4CounteffectiveDate { get; set; }

        [DisplayName("CD4%")]
        public decimal? CD4Percentage { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CD4PercentageeffectiveDate { get; set; }

        [DisplayName("Viral load")]
        public decimal? viralLoad { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? viralLoadeffectiveDate { get; set; }

        [DisplayName("Haemoglobin")]
        public decimal? haemoglobin { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? haemoglobineffectiveDate { get; set; }

        [DisplayName("Bilirubin")]
        public decimal? bilirubin { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? bilirubineffectiveDate { get; set; }

        [DisplayName("Cholesterol")]
        public decimal? totalCholestrol { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? totalCholestroleffectiveDate { get; set; }

        [DisplayName("HDL")]
        public decimal? hdl { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hdleffectiveDate { get; set; }

        [DisplayName("LDL")]
        public decimal? ldl { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ldleffectiveDate { get; set; }

        [DisplayName("Triglycerides")]
        public decimal? triglycerides { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? triglycerideseffectiveDate { get; set; }

        [DisplayName("Glucose")]
        public decimal? glucose { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? glucoseeffectiveDate { get; set; }

        [DisplayName("HbA1c")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:0.00}%")]
        public decimal? hba1c { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hba1ceffectiveDate { get; set; }

        [DisplayName("ALT")]
        public decimal? alt { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? alteffectiveDate { get; set; }

        [DisplayName("AST")]
        public decimal? ast { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? asteffectiveDate { get; set; }

        [DisplayName("S-Urea")]
        public decimal? urea { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ureaeffectiveDate { get; set; }

        [DisplayName("S-Creatinine")]
        public decimal? creatinine { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? creatinineeffectiveDate { get; set; }

        [Required(ErrorMessage = "Please Insert Lab Name!")]
        [DisplayName("Lab")]
        public string labName { get; set; }

        [Required(ErrorMessage = "Please Insert Lab Reference!")]
        [DisplayName("Ref #")]
        public string labReferenceNo { get; set; }

        [DisplayName("Lab Tel")]
        public string labTelNo { get; set; }

        [DisplayName("eGFR")]
        public decimal? eGfr { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? eGfreffectiveDate { get; set; }

        [DisplayName("U-Albumin to Creat ratio")]
        public decimal? mauCreatRatio { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? mauCreatRatioeffectiveDate { get; set; }

        [DisplayName("Sys BP")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0}")]
        public decimal? systolicBP { get; set; }

        [DisplayName("Dia BP")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0}")]
        public decimal? diastolicBP { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BPeffectiveDate { get; set; }

        [DisplayName("FEV1")]
        public decimal? FEV1 { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FEV1effectiveDate { get; set; }

        [DisplayName("Eosinophylia")]
        public decimal? Eosinophylia { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EosinophyliaeffectiveDate { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        public string userInitial
        {
            get
            {
                return createdBy.ToUpper().Substring(0, 1);
            }
        }


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

        [DisplayName("HIV Elisa")]
        public string hivEliza { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hivElizaeffectiveDate { get; set; }

        [DisplayName("Normal GTT")]
        public string normGtt { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? normGtteffectiveDate { get; set; }

        [DisplayName("Abnormal GTT")]
        public string abnGtt { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? abnGtteffectiveDate { get; set; }

        [DisplayName("U-Creatinine")]
        public decimal? ucreatinine { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ucreatinineeffectiveDate { get; set; }

        [DisplayName("S-Albumin")]
        public decimal? salbumin { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? salbumineffectiveDate { get; set; }

        [DisplayName("U-Albuminuria")]
        public decimal? ualbuminuria { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ualbuminuriaeffectiveDate { get; set; }

        public int sentToCL { get; set; }

        public bool stageChecked { get; set; }

        public bool HypoChecked { get; set; }
        public bool egfrChecked { get; set; }

        [NotMapped]
        public string PathologyField { set; get; } //HCare-973
        [NotMapped]
        public int PathologyFieldDueDate { set; get; }
    }
}