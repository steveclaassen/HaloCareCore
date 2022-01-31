using HaloCareCore.Models.Management;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class PatientClinicalSummaryViewModel
    {

        public List<Clinical> clinicalexams { get; set; }
        public List<Clinical> generalClinicalExams { get; set; }
        public List<Clinical> diabetesClinicalExams { get; set; }
        public List<Clinical> hivClinicalExams { get; set; }
        //-->
        public List<MedicationHistory> meds { get; set; }
        public List<MedicationHistory> generalMedications { get; set; }
        public List<MedicationHistory> diabetesMedications { get; set; }
        public List<MedicationHistory> hivMedications { get; set; }
        //-->
        public List<Allergies> allergies { get; set; }
        public List<Allergies> generalAllergies { get; set; }
        //-->
        public ClinicalHistoryQuestionaire questionaire { get; set; }
        public List<ClinicalHistoryQuestionaire> questionaires { get; set; }
        public List<ClinicalHistoryQuestionaire> generalQuestionaires { get; set; }
        //-->
        public List<HospitalizationAuths> generalHospitalAuths { get; set; }
        public List<HospitalizationAuths> diabetesHospitalAuths { get; set; }
        public List<HospitalizationAuths> hivHospitalAuths { get; set; }
        //-->
        public List<QuestionnaireOther> questionnaireOthers { get; set; }
        public List<QuestionnaireOther> generalQuestionnaireOthers { get; set; }
        public List<QuestionnaireOther> diabetesQuestionnaireOthers { get; set; }
        public List<QuestionnaireOther> hivQuestionnaireOthers { get; set; }
        //-->
        public List<QuestionnaireOther> Other { get; set; }

        //-->
        public List<QuestionnaireOther> Pregnancy { get; set; }
        //-->
        public List<NewBorn> NewBorns { get; set; }
        //-->
        //public List<PatientDiagnosis> programDiagnoses { get; set; } //hcare-1312: removed 17/08/2021
        //public List<PatientDiagnosis> diabetesProgramDiagnoses { get; set; } //hcare-1312: removed 17/08/2021
        //public List<PatientDiagnosis> hivProgramDiagnoses { get; set; } //hcare-1312: removed 17/08/2021

        public List<PatientDiagnosis> programDiagnoses { get; set; } //hcare-1312
        public List<PatientDiagnosis> diabetesProgramDiagnoses { get; set; } //hcare-1312
        public List<PatientDiagnosis> hivProgramDiagnoses { get; set; } //hcare-1312

        //-->

        //Diabetes ONLY tables
        public List<Vision> visions { get; set; }
        public Podiatry podiatry { get; set; }
        public List<Podiatry> podiatries { get; set; }
        public List<AutoNeropathy> autoNeropathies { get; set; }
        public List<Hypoglycaemia> hypoglycaemias { get; set; }

        //HIV ONLY tables
        public List<OtherMedicalHistory> otherMedicalHistories { get; set; }

        //Mental health tables
        public List<MH_DSM5Response> MH_DSM5Responses { get; set; }
        public List<MH_SchizophreniaResponse> MH_SchizophreniaResponses { get; set; }
        public List<MH_BipolarResponse> MH_BipolarResponses { get; set; }
        public List<MH_DepressionResponse> MH_DepressionResponses { get; set; }


        //-->

        public List<ScriptViewModel> ScriptItems { get; set; }
        public List<DoctorQuestionnaireResults> doctorQuestionnaireResults { get; set; }

        #region diebetes care plan hcare-1092
        public List<CarePlanPathology> diabetesCarePlanPathologies { get; set; }
        public CarePlanPathology DiabetesCarePlanPathology { get; set; }
        public ClinicalAddition ClinicalAddition { set; get; }
        public List<ClinicalAddition> clinicalAdditions { get; set; }
        public NutritionAndLifestyle NutritionAndLifestyle { set; get; }
        public List<NutritionAndLifestyle> nutritionAndLifestyles { get; set; }
        public Visit Visit { get; set; }
        public List<Visit> visits { set; get; }
        public Medicine Medicine { set; get; }
        public List<Medicine> medicines { set; get; }
        public paediatric paediatric { set; get; }
        public List<paediatric> paediatrics { set; get; }
        #endregion


    }

}