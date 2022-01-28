using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_BipolarResponse
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        
        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }

        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Depression")]
        public string Depression { get; set; }
        
        [DisplayName("Score")]
        public int DepressionSC { get; set; }

        [DisplayName("Insomnia")]
        public string Insomnia { get; set; }

        [DisplayName("Score")]
        public int InsomniaSC { get; set; }

        [DisplayName("Appetite")]
        public string Appetite { get; set; }
        
        [DisplayName("Score")]
        public int AppetiteSC { get; set; }

        [DisplayName("Social")]
        public string SocialEngagement { get; set; }

        [DisplayName("Score")]
        public int SocialEngagementSC { get; set; }

        [DisplayName("Activity")]
        public string Activity { get; set; }
        
        [DisplayName("Score")]
        public int ActivitySC { get; set; }

        [DisplayName("Motivation")]
        public string Motivation { get; set; }
        
        [DisplayName("Score")]
        public int MotivationSC { get; set; }

        [DisplayName("Conc.")]
        public string Concentration { get; set; }
        
        [DisplayName("Score")]
        public int ConcentrationSC { get; set; }

        [DisplayName("Anxiety")]
        public string Anxiety { get; set; }

        [DisplayName("Score")]
        public int AnxietySC { get; set; }

        [DisplayName("Pleasure")]
        public string Pleasure { get; set; }
        
        [DisplayName("Score")]
        public int PleasureSC { get; set; }

        [DisplayName("Emotion")]
        public string Emotion { get; set; }
        
        [DisplayName("Score")]
        public int EmotionSC { get; set; }

        [DisplayName("Worth")]
        public string Worthlessness { get; set; }

        [DisplayName("Score")]
        public int WorthlessnessSC { get; set; }

        [DisplayName("Helpless")]
        public string Helplessness { get; set; }
        
        [DisplayName("Score")]
        public int HelplessnessSC { get; set; }

        [DisplayName("Suicidal")]
        public string Suicidal { get; set; }

        [DisplayName("Score")]
        public int SuicidalSC { get; set; }

        [DisplayName("Suicidal comment")]
        public string SuicidalCMT { get; set; }

        [DisplayName("Guilt")]
        public string Guilt { get; set; }
        
        [DisplayName("Score")]
        public int GuiltSC { get; set; }

        [DisplayName("Psychotic")]
        public string Psychotic { get; set; }
        
        [DisplayName("Score")]
        public int PsychoticSC { get; set; }

        [DisplayName("Irritable")]
        public string Irritability { get; set; }
        
        [DisplayName("Score")]
        public int IrritabilitySC { get; set; }

        [DisplayName("Lability")]
        public string Lability { get; set; }
        
        [DisplayName("Score")]
        public int LabilitySC { get; set; }

        [DisplayName("Motor")]
        public string IncMotorDrive { get; set; }
        
        [DisplayName("Score")]
        public int IncMotorDriveSC { get; set; }

        [DisplayName("Speech")]
        public string IncSpeech { get; set; }

        [DisplayName("Score")]
        public int IncSpeechSC { get; set; }

        [DisplayName("Agitation")]
        public string Agitation { get; set; }
        [DisplayName("Score")]
        public int AgitationSC { get; set; }

        [DisplayName("Outcome")]
        public string Outcome { get; set; }
        
        [DisplayName("Total score")]
        public int TotalScore { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Follow up")]
        public bool FollowUp { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public int? RiskID { get; set; } //HCare-1151

    }
}
