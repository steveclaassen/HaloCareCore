using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models
{
    public class AdvancedSearchResultModel
    {
        public Guid dependantID { get; set; }

        public Guid medicalAidID { get; set; }
        public Guid programId { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("ID/Auth #")]
        public string idNumber { get; set; }

        [DisplayName("Medical scheme")]
        public string medicalAidCode { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Member name")]
        public string memberName_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(memberName))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(memberName.ToUpper().Substring(0, 1) + memberName.ToLower().Substring(1));
                }
                else
                {
                    return memberName;
                }
            }

        }

        [DisplayName("Member surname")]
        public string memberSurname { get; set; }

        [DisplayName("Member surname")]
        public string memberSurname_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(memberSurname))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(memberSurname.ToUpper().Substring(0, 1) + memberSurname.ToLower().Substring(1));
                }
                else
                {
                    return memberSurname;
                }
            }
        }

        [DisplayName("Birth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? birthDate { get; set; }

        [DisplayName("Scheme option")]
        public string schemeOption { get; set; }

        [DisplayName("Patient status")]
        public string PatientStatus { get; set; } //hcare-1314 

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("ICD-10")] //HCare-958
        public string icd10Code { get; set; }

        [DisplayName("Program start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? programStartDate { get; set; }

        [DisplayName("Program end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ProgramEndDate { get; set; }


        [DisplayName("Member status")]
        public string memberStatus { get; set; }

        [DisplayName("Status effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StatusEffectiveDate { get; set; }

        [DisplayName("Status end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StatusEndDate { get; set; }

        [DisplayName("Modified by")]
        public string statusModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? statusModifiedDate { get; set; }

        [DisplayName("BHF #")]
        public string drBHFNumber { get; set; }

        [DisplayName("Doctor full names")]
        public string DoctorFullNames { set; get; } //HCare-1423
        [DisplayName("Doctor name")]
        public string drName { get; set; }

        [DisplayName("Doctor surname")]
        public string drSurname { get; set; }

        [DisplayName("Doctor email")]
        public string drEmail { get; set; }
        [DisplayName("Doctor Cell")]
        public string drCell { get; set; }

        [DisplayName("Last lab name")]
        public string latestPathologyLab { get; set; }

        [DisplayName("Case manager")]
        public string CaseManager { get; set; }

        [DisplayName("Member cell")]
        public string cell { get; set; }

        [DisplayName("Member tel")]
        public string tel { get; set; }

        public string statusCode { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        //[DisplayName("Risk Code")]
        //public string RiskRating { get; set; }

        //[DisplayName("Risk category")]
        //public string RiskType { get; set; }

        //[DisplayName("Risk priority")]
        //public string RiskPriority { get; set; }

        [DisplayName("Risk rating history")]
        public string RiskRatingHistory { get; set; } //hcare-1074

        [DisplayName("Risk Priority History")]
        public string RiskPriorityHistory { get; set; } //hcare-1074

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? RiskEffectiveHistory { get; set; } //hcare-1074

        [DisplayName("Risk rating Current")]
        public string RiskRatingCurrent { get; set; } //hcare-1074

        [DisplayName("Risk priority Current")]
        public string RiskPriorityCurrent { get; set; } //hcare-1074

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? RiskEffectiveCurrent { get; set; } //hcare-1074

        public string OverallRating { get; set; }

        public string OverallPriority { get; set; }

        [DisplayName("HbA1c")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:0.00}%")]
        public decimal? hba1c { get; set; }

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hba1ceffectiveDate { get; set; }

        [DisplayName("Viral load")]
        public decimal? viralLoad { get; set; }

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? viralLoadeffectiveDate { get; set; }

        [DisplayName("Cervical cancer")]
        public bool cervicalCancer { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? cervicalCancerEffectiveDate { get; set; }

        [DisplayName("Cervical cancer result")]
        public string cervicalCancerComment { get; set; }

        [DisplayName("Frail care")]
        public bool frailCareCheck { get; set; }

        [DisplayName("Frail care Assistance")]
        public string frailCare { get; set; }

        [DisplayName("Frail care Comment")]
        public string frailCareComment { get; set; }

        [DisplayName("TB screened")]
        public string TBScreened { get; set; } //HCare-966
        public string TBScreenResult { get; set; } //HCare-1275
        public DateTime? TBTreatmentStart { get; set; } //HCare-1275
        public DateTime? TBTreatmentEnd { get; set; } //HCare-1275

        public string SuicideRiskCurrent { get; set; }
        public string SuicideRiskHistory { get; set; }
        public string DepressionCurrent { get; set; }
        public string DepressionHistory { get; set; }
        public string DSM5Current { get; set; }
        public string DSM5History { get; set; }
        public string DrReferral { get; set; }
        public DateTime? DrReferralDate { get; set; }
        public string ReferralFrom { get; set; }

        [DisplayName("HIV Stage")]
        public string stageCode { get; set; }  //hcare-1014

        public string StateEnrolled { get; set; } //hcare-863
        public string EmployerCode { get; set; } //hcare-1373
        public string EmployerCodeDescription { get; set; } //hcare-1373



    }
}