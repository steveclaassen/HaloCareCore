using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class DoctorQuestionnaireResults
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [DisplayName("Diagnosis date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? diagnosisDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool Pregnant { get; set; }

        [DisplayName("Expected delivery")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EDD { get; set; }

        [DisplayName("Partner status")]
        public string PartnerStatus { get; set; }

        [DisplayName("WHO stage")]
        public string whoStage { get; set; }

        [DisplayName("Yes/No")]
        public bool GenotypingDone { get; set; }

        [DisplayName("GenoTyping date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? GenotyingDate { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool tbDiagnosed { get; set; }

        [DisplayName("TB treatement start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? tbTreatmentStartDate { get; set; }

        [DisplayName("TB treatement end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? tbTreatmentEndDate { get; set; }

        [DisplayName("Yes/No")]
        public bool PEP { get; set; }

        [DisplayName("PEP date of exposrue")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? pepDateofExposure { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool recDrugs { get; set; }

        [DisplayName("Recreational drugs last use")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? recDrugsLastUsed { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool depression { get; set; }

        [DisplayName("Comment")]
        public string depressionComment { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

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


        //Diabetes additions

        [DisplayName("Amputation comment")]
        public string amputationComment { get; set; }

        [DisplayName("Amputation reason")]
        public string amputationReason { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool amputationCheck { get; set; }

        [DisplayName("Podiatry diabetic Foot Risk ")]
        public string podiatryDiabetic { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryDiabeticCheck { get; set; }

        [DisplayName("Podiatry vascular comment")]
        public string podiatryVascular { get; set; }

        [DisplayName("Yes/No")]
        public bool podiatryVascularCheck { get; set; }

        [DisplayName("Podiatry Neurological comment")]
        public string podiatryNeurological { get; set; }

        [DisplayName("Yes/No")]
        public bool podiatryNeurologicalCheck { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryLopsCheck{ get; set; }

        [DisplayName("LOPS comment")]
        public string podiatryLopsComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryDeformityCheck { get; set; }

        [DisplayName("Deformation comment")]
        public string podiatryDeformityComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryPerArterialDiseaseCheck { get; set; }

        [DisplayName("Peripheral Arterial Disease")]
        public string podiatryPerArterialDiseaseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool podiatryWoundCheck { get; set; }

        [DisplayName("Podiatry Wound Comment")]
        public string podiatryWoundComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool autoNeuroSympCheck { get; set; }

        [DisplayName("Auto Nero Symptoms")]
        public string autoNeuroSympComment { get; set; }
                
        [DisplayName("How is your vision")]
        public string vision { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool visionCheck { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool eyeTreatmentCheck { get; set; }

        [DisplayName("Eye treatments")]
        public string eyeTreatments { get; set; }

        [DisplayName("What is your occupation")]
        public string occupation { get; set; }

        [DisplayName("Yes/No")]
        public bool occupationCheck { get; set; }

        [DisplayName("Shift worker comment")]
        public string shiftWorker { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool shiftWorkCheck { get; set; }

        [DisplayName("Lypohypertrophy comment")]
        public string lypohypertrophy { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool lypohypertrophyCheck { get; set; }

        [DisplayName("Lypohypertrophy diagnosed")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? lypohypertrophyDate { get; set; }

        [DisplayName("Glucose reading below 4.1")]
        public string glucoseReading { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool hypoglycaemiaSymptomsCheck{ get; set; }

        [DisplayName("Yes/No")]
        public bool hypoglycaemiaCheck { get; set; }

        [DisplayName("Hypoglycaemic risk comment")]
        public string hypoglycaemiaComment { get; set; }

        [DisplayName("Hypoglycaemic symptoms comment")]
        public string hypoglycaemiaSymptomsComment { get; set; }

        [DisplayName("Hypoglycaemic related assistance")]
        public string hypoglycaemiaAssistance{ get; set; }

        [DisplayName("Emergency room visits")]
        public string emergencyRoomVisits { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool insulinUseCheck { get; set; }

        [DisplayName("Insulin Comment")]
        public string insulinUseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool sulfonylureaUseCheck { get; set; }

        [DisplayName("Insulin Comment")]
        public string sulfonylureaUseComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool ckdStageCheck { get; set; }

        [DisplayName("End-Stage kidney disease")]
        public string ckdStageComment { get; set; }

        [DisplayName("Yes/No")]
        [Required(ErrorMessage = "Yes/No required")]
        public bool patientAgeCheck { get; set; }

        [Required]
        public bool Active { get; set; }

        //Navigational Properties
        public int? AllergyID { get; set; }

        public int? SocialHistoryID { get; set; }

        public int? PodiatryID { get; set; }

        public int? NervousSystemID { get; set; }

        public int? AutoNeropathyID { get; set; }

        public int? VisionID { get; set; }

        public int? HypoglycaemiaID { get; set; }

        public int? QuestionnaireOtherID { get; set; }

        public int? ClinicalHistoryID { get; set; }
        public int? NewBornID { get; set; }





    }
}
