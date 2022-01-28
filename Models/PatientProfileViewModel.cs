using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class PatientProfileViewModel
    {
        //basic patient information
        [DisplayName("Valid ID")]
        public bool isValidId { get; set; }

        [DisplayName("Last claim")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lastClaimDate { get; set; }

        [DisplayName("Next claim")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? nextClaimDate { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? scriptEffectiveDate { get; set; }

        [DisplayName("Script repeats")]
        public int scriptRepeats { get; set; }

        [DisplayName("Claims")]
        public int claims { get; set; }

        [DisplayName("New script")]
        public bool requiresNewScript { get; set; }

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

        //open assignments & enquires
        public List<Assignments> assignments { get; set; }
        public List<Queries> queries { get; set; }

        //attachment
        public Attachments attachment { get; set; }
        public List<Attachments> attachments { get; set; }

        public AuthorizationLetters authLetter { get; set; }
        public List<AuthorizationLetters> authLetters { get; set; }

        public PatientQuestionnaireResponse disclaimerResult { get; set; }
        public List<PatientQuestionnaireResponse> disclaimerResults { get; set; }

        public disclaimerViewModel disclaimerViewModel { get; set; }
        public List<disclaimerViewModel> disclaimerViewModels { get; set; }

        public List<MedicalAidPlans> patientPlans { get; set; }

        public PatientPlanHistory patientPlanHistory { get; set; }
        public List<PatientPlanHistory> patientPlanHistories { get; set; }

        public List<PayPoints> paypoints { get; set; }
        public PayPointHistory payPointHistory { get; set; }
        public List<PayPointHistory> payPointHistories { get; set; }

        public PatientDisclaimer patientDisclaimer { get; set; }
        public List<PatientDisclaimer> patientDisclaimers { get; set; }

        //questionnaireIndex
        public DoctorQuestionnaireResults doctorQuestionnaireResult { get; set; }
        public List<DoctorQuestionnaireResults> doctorQuestionnaireResults { get; set; }


    }
}