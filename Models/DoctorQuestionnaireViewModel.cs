using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System.Collections.Generic;

namespace HaloCareCore.Models
{
    public class DoctorQuestionnaireViewModel
    {
        public Clinical clinicalexam { get; set; }
        public List<Clinical> clinicalexams { get; set; }

        public MedicationHistory MedicationHistory { get; set; }
        public List<MedicationHistory> MedicationHistories { get; set; }

        public Allergies allergy { get; set; }
        public List<Allergies> allergies { get; set; }

        public ClinicalHistoryQuestionaire questionnaire { get; set; }
        public List<ClinicalHistoryQuestionaire> questionnaires { get; set; }

        public HospitalizationAuths HospitalAuth { get; set; }
        public List<HospitalizationAuths> HospitalAuths { get; set; }

        public CoMormidConditions comormidCondition { get; set; }
        public List<CoMormidConditions> comormidConditions { get; set; }

        public CoMorbidTypes coMorbidType { get; set; }
        public List<CoMorbidTypes> coMorbidTypes { get; set; }

        public CoMorbidConditionVM comorbidCondition { get; set; }
        public List<CoMorbidConditionVM> comorbidConditions { get; set; }

        public Pathology pathology { get; set; }
        public List<Pathology> pathologies { get; set; }
        public Pathology hivPathology { get; set; }
        public List<Pathology> hivPathologies { get; set; }

        public PatientProgramHistory programHistory { get; set; }

        public DoctorQuestionnaireResults drquestionnaireResults { get; set; }
        public List<DoctorQuestionnaireResults> drquestionnaireResultList { get; set; }

        public PatientDiagnosis PatientDiagnosis { get; set; } //hcare-1312
        public List<PatientDiagnosis> programDiagnoses { get; set; } //hcare-1312

        public ComorbidView ComorbidView { get; set; }
        public List<ComorbidView> ComorbidViews { get; set; }
        public ComorbidView ComorbidViewX { get; set; }
        public List<ComorbidView> ComorbidViewsX { get; set; }

        public Vision vision { get; set; }
        public List<Vision> visions { get; set; }
        public Podiatry podiatry { get; set; }
        public List<Podiatry> podiatries { get; set; }
        
        public AutoNeropathy autoNeropathy { get; set; }
        public List<AutoNeropathy> autoNeropathies { get; set; }

        public Hypoglycaemia hypoglycaemia { get; set; }
        public List<Hypoglycaemia> hypoglycaemias { get; set; }

        public QuestionnaireOther questionnaireOther { get; set; }
        public List<QuestionnaireOther> questionnaireOthers { get; set; }
        public QuestionnaireOther diabQuestionnaireOther { get; set; }
        public List<QuestionnaireOther> diabQuestionnaireOthers { get; set; }
        public QuestionnaireOther hivQuestionnaireOther { get; set; }
        public List<QuestionnaireOther> hivQuestionnaireOthers { get; set; }

        public Notes Note { get; set; }
        public List<Notes> Notes { get; set; }

        public ScriptCreationViewModel Script { get; set; }
        public ScriptViewModel ScriptVMItem { get; set; }
        public List<ScriptViewModel> ScriptItems { get; set; }
        public List<ScriptViewModel> generalScriptItems { get; set; }

        public ScriptItems ScriptItem { get; set; }
        public List<ScriptItems> theScriptItems { get; set; }
        public List<Scripts> Scripts { get; set; }

        public Doctors doctor { get; set; }
        public List<Doctors> doctors { get; set; }

        public Contact doctorContact { get; set; }
        public List<Contact> doctorContacts { get; set; }

        public Member member { get; set; }
        public List<Member> members { get; set; }

        public Dependant dependent { get; set; }
        public List<Dependant> dependents { get; set; }

        public MedicalAid medicalAid { get; set; }
        public Address address { get; set; }

        public Contact contact { get; set; }
        public List<Contact> contacts { get; set; }

        public PatientDisclaimer patientDisclaimer { get; set; }
        public List<PatientDisclaimer> patientDisclaimers { get; set; }

        public OtherMedicalHistory otherMedicalHistory { get; set; }
        public List<OtherMedicalHistory> otherMedicalHistories { get; set; }

        public NewBorn NewBorn { get; set; }
        public List<NewBorn> NewBorns { get; set; }



    }
}
