using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class RiskSearch
    {
        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("ICD-10")]
        public string ICD10 { get; set; }

        [DisplayName("Patient name")]
        public string PatientName { get; set; }

        [DisplayName("ID/Auth #")]
        public string IDNumber { get; set; }

        [DisplayName("Member number")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("Birth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }

        [DisplayName("Gender")]
        public string Gender { get; set; }

        [DisplayName(">=65 years")]
        public string Age65 { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        //-->> Risk rating

        [DisplayName("Risk ID")]
        public string RiskID { get; set; }

        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("Reason")]
        public string RiskReason { get; set; }

        [DisplayName("Program code")]
        public string ProgramCode { get; set; }

        //-->> Other

        public string HypoRisk { get; set; }
        public string VirtualDisorder { get; set; }
        public string DiabeticRetinopathy { get; set; }
        public string Cardiomyopathy { get; set; }
        public string CAD { get; set; }
        public string CCF { get; set; }
        public string Dysrhythmia { get; set; }
        public string ChronicRenalFailure { get; set; }
        public string Amputation { get; set; }
        public string Hyperlipidaemia { get; set; }
        public string Hypertension { get; set; }
        public string HypoglycaemicUnaware { get; set; }
        public string DiabeticDiary { get; set; }
        public string Smoker { get; set; }
        public string Tuberculosis { get; set; }
        public string Cancer { get; set; }

        //-->> Pathology

        [DisplayName("eGFR")]
        public decimal? eGFR { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? eGFREffectiveDate { get; set; }

        [DisplayName("HbA1c")]
        public decimal? HbA1c { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? HbA1cEffectiveDate { get; set; }

        [DisplayName("Viral Load")]
        public decimal? ViralLoad { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ViralLoadEffectiveDate { get; set; }

        [DisplayName("CD4 count")]
        public decimal? CD4Count { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CD4CountEffectiveDate { get; set; }

        [DisplayName("CD4 percentage")]
        public decimal? CD4Percentage { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CD4PercentageEffectiveDate { get; set; }

        //--
        public Guid DependantID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        public bool Active { get; set; }

    }
}
