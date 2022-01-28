﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels.AdvancedSearchLiteViewModel
{
    public class MHAdvancedSearchResults
    {
        public Guid dependantID { get; set; }


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
        public DateTime birthDate { get; set; }

        [DisplayName("Scheme option")]
        public string schemeOption { get; set; }

        [DisplayName("Patient status")]
        public string PatientStatus { get; set; } 

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("ICD-10")] 
        public string icd10Code { get; set; }

        [DisplayName("Program start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime programStartDate { get; set; }

        [DisplayName("Program end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ProgramEndDate { get; set; }


        [DisplayName("Member status")]
        public string memberStatus { get; set; }

        [DisplayName("Status effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StatusEffectiveDate { get; set; }

        [DisplayName("Status end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StatusEndDate { get; set; }

        [DisplayName("Modified by")]
        public string statusModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime statusModifiedDate { get; set; }

        [DisplayName("BHF #")]
        public string drBHFNumber { get; set; }

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

        [DisplayName("Risk rating history")]
        public string RiskRatingHistory { get; set; } 

        [DisplayName("Risk Priority History")]
        public string RiskPriorityHistory { get; set; } //hcare-1074

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RiskEffectiveHistory { get; set; } //hcare-1074

        [DisplayName("Risk rating Current")]
        public string RiskRatingCurrent { get; set; } //hcare-1074

        [DisplayName("Risk priority Current")]
        public string RiskPriorityCurrent { get; set; } //hcare-1074

        [DisplayName("Effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RiskEffectiveCurrent { get; set; } //hcare-1074
        public string OverallRating { get; set; }

        public string SuicideRiskCurrent { get; set; }
        public string SuicideRiskHistory { get; set; }
        public string DepressionCurrent { get; set; }
        public string DepressionHistory { get; set; }
        public string DSM5Current { get; set; }
        public string DSM5History { get; set; }
        public string DrReferral { get; set; }
        public DateTime DrReferralDate { get; set; }
        public string ReferralFrom { get; set; }
        public string StateEnrolled { get; set; } 
        public string EmployerCode { get; set; } 
        public string EmployerCodeDescription { get; set; } 
        public string Outcome { set; get; }

    }
}