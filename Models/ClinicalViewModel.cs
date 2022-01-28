using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models
{
    public class ClinicalViewModel
    {
        public Guid DependentID { get; set; }

        //claim and script information
        [DisplayName("Last claim")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lastClaimDate { get; set; }

        [DisplayName("Next claim")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? nextClaimDate { get; set; }

        [DisplayName("No of claims")]
        public int nrofclaims { get; set; }

        [DisplayName("Reorder")]
        public bool dueForReorder { get; set; }

        //==>
        [DisplayName("Script effective")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? scriptEffectiveDate { get; set; }

        [DisplayName("Script repeats")]
        public int scriptRepeats { get; set; }

        [DisplayName("Script")]
        public bool hasScript { get; set; }

        [DisplayName("Expiry")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? scriptExpiry { get; set; }

        [DisplayName("Created by")]
        public string scriptCreatedBy { get; set; }

        [DisplayName("New script")]
        public bool requiresNewScript { get; set; }

        //==>
        //pathology information
        [DisplayName("Next pathology")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? nextPathologyDate { get; set; }

        [DisplayName("Last pathology")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lastPathologyDate { get; set; }

        [DisplayName("Pathology due")]
        public bool isPathologyDue { get; set; }

        [DisplayName("Pathology behind")]
        public int pathologyBehind { get; set; }

        [DisplayName("Pathology")]
        public bool hasPathology { get; set; }

        //==>
        public ClaimsHistory claimHistory { get; set; }
        public PatientClinicalSummaryViewModel summary { get; set; }
        //==>
        public Pathology generalPathology { get; set; }
        public List<Pathology> generalPathologies { get; set; }
        public List<Pathology> Pathologies { get; set; }
        public Pathology hivPathology { get; set; }
        public List<Pathology> hivPathologies { get; set; }
        public Pathology diabetesPathology { get; set; }
        public List<Pathology> diabetesPathologies { get; set; }
        public Pathology hyperlipidaemiaPathology { get; set; }
        public List<Pathology> hyperlipidaemiaPathologies { get; set; }
        public Pathology hivBloodResult { get; set; }
        public List<Pathology> hivBloodResults { get; set; }
        //==>
        public Scripts Script { get; set; }
        public List<Scripts> Scripts { get; set; }
        public ScriptViewModel ScriptItem { get; set; }
        public List<ScriptViewModel> ScriptItems { get; set; }
        public List<ScriptViewModel> generalScriptItems { get; set; }
        public List<ScriptViewModel> hivScriptItems { get; set; }
        public List<ScriptViewModel> diabetesScriptItems { get; set; }
        public List<ScriptViewModel> MentalHealthScriptItems { get; set; }
        public List<ScriptViewModel> ScriptItemsFull { get; set; }
        public List<Clinical> clinicalExams { get; set; }
        public List<HospitalizationAuths> HospitalAuths { get; set; }
        public List<HospitalizationAuths> generalHospitalAuths { get; set; }
        public List<HospitalClaimView> HospitalClaims { get; set; }
        public List<ScriptReferences> scriptReferences { get; set; }
        public List<RulesInstructions> rulesBroken { get; set; }
        public List<ComorbidView> CoMorbids { get; set; }
        public List<ComorbidView> generalCoMorbids { get; set; }

        public CoMorbidTypes coMorbidType { get; set; }
        public List<CoMorbidTypes> coMorbidTypes { get; set; }
        public List<PatientRiskRatingHistory> RiskRating { get; set; }

        public PatientProgramHistory program { get; set; }
        public List<PatientProgramHistory> programs { get; set; }

        public ClinicalRules clinicalRule { get; set; }
        public List<ClinicalRules> clinicalRules { get; set; }
        public List<ClinicalRulesVM> clinicalRulesVM { get; set; }
        public List<ClinicalRulesVM> BrokenRules { get; set; }

        public string programcode { get; set; }
        public string gender { get; set; }


        public List<PathologySearch> PathologySearches { get; set; } //HCare-974
        public List<DoctorReferral> DoctorReferrals { get; set; } //HCare-1136

        public IEnumerable<PatientStagingHistoryViewModel> patientStagingHistories { get; set; }

        public MHDiagnosisVM MHDiagnosis { get; set; } //HCare-1194
        public List<MHDiagnosisVM> MHDiagnoses { get; set; } //HCare-1194

        public List<MH_DSM5Score> DSM5Scores { get; set; } //HCare-1206

        public List<Hba1cRangeHistory> RangeHistory { get; set; } //HCare-1241

        public QuestionnaireOther TBPathology { get; set; } //HCare-1279
        public List<QuestionnaireOther> TBPathologies { get; set; } //HCare-1279

        public List<InsulinDependancyVM> InsulinDependancy { get; set; } //hcare-673
        public List<InsulinDependance> Categories { get; set; } //hcare-673



        public bool CanCreate { set; get; }
        public bool CanEdit { set; get; }
        public bool CanRead { set; get; }
        public List<TariffClaimHistory> TariffClaimHistories { set; get; } //HCare-1016
    }
}