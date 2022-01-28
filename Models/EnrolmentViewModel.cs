using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models
{
    public class EnrolmentViewModel
    {
        public bool hasSteps { get; set; }

        public string plan { get; set; }

        public string paypoint { get; set; }

        public string patientStatusName { get; set; }

        public Member member { get; set; }

        public Dependant dependent { get; set; }

        public Address address { get; set; }

        public MemberInformation memberInformation { get; set; }

        public EnrolmentStepsMonitor enroll { get; set; }

        public Notes Notes { get; set; }

        public PatientDispensingProviderHistory DProvider { get; set; }

        public CaseManagerHistory CaseManagers { get; set; }

        public List<PatientManagementStatusHistory> statuss { get; set; }

        public MedicalAid medicalAid { get; set; }
        public List<MedicalAid> MedicalAids { get; set; }

        public List<Origin> Origins { get; set; }

        public List<Icd10Codes> IcdCodes { get; set; }

        public Doctors doctor { get; set; }
        public List<Doctors> doctors { get; set; }

        public Language language{ get; set; }
        public List<Language> languages { get; set; }

        public List<DispensingProviders> providers { get; set; }

        public List<Sex> gender { get; set; }

        public List<DependantType> dependentType { get; set; }

        public Contact contact { get; set; }
        public Contact doctorContact { get; set; }
        public List<Contact> doctorContacts { get; set; }

        public Programs Prog { get; set; }
        public List<Programs> Progs { get; set; }

        public PatientProgramHistory program { get; set; }
        public List<PatientProgramHistory> programs { get; set; }
        public List<PatientProgramSubHistory> SubPrograms { get; set; }
        public List<PatientDiagnosis> MentalHealthDiagnoses { get; set; } //hcare-1312: correction
        public List<PatientProgramHistory> PatientProgramHistories { get; set; }
        public List<PatientProgramVM> programHistories { get; set; } //hcare-1203

        public string RiskRating { get; set; }
        public string RiskReason { get; set; }
        public string RiskProgram { get; set; }
        public string programcode { get; set; }
        public string HypoRisk { get; set; }

        public HypoglymiaRiskHistory hypoglymiaRiskHistory { get; set; }

        public List<MembershipNoHistory> membershipNoHistories { get; set; }
        public List<MemberImports> memberImports { get; set; }

        public string programID { get; set; }

        public string memberLanguage { get; set; }
        public string memberGender { get; set; }
        public string memberDependantType { get; set; }

        public decimal? hba1c { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hbEffective { get; set; }


        public List<HospitalizationAuths> hospitalAuths { get; set; }

        public PatientStagingHistory PatientStagingHistory { set; get; } //HCare-1014

        public UserMemberHistory UserMemberHistory { get; set; }
        public List<UserMemberHistory> UserMemberHistories { get; set; }

        public wfUserVM wfQueue { get; set; }
        public List<wfUserVM> wfQueues { get; set; }

        public QuestionnaireOther QuestionnaireOther { get; set; } //HCare-1276
        public PatientDiagnosis StateEnrolled { get; set; } //hcare-863 //hcare-1312

        public List<InsulinDependanceHistory> InsulinDependantList { get; set; } //hcare-673
 


        //HCare-996
        public bool CanRead { set; get; }
        public bool CanEdit { set; get; }
        public bool CanCreate { set; get; }

        public PopUpTemplate PopUpTemplate { get; set; } //hcare-1374
        public List<PopUpTemplate> PopUpTemplates { get; set; } //hcare-1374
        public PopUpTemplateVM PopUp { get; set; } //hcare-1374
        public bool HasICD10OnDischarge { get; set; } //Hcare-1481
        public ICD10OnDischargeSetup ICD10OnDischargeSetup { get; set; } //HCare-1481


    }
}