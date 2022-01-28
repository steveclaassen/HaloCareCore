using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class MH_DSM5ResponseNEW
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }

        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }

        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Substance abuse")]
        public bool SubstanceAbuse { get; set; }

        [DisplayName("Dep 1")]
        public string DepressionOne { get; set; }

        [DisplayName("Score")]
        public int DepressionOneSC { get; set; }

        [DisplayName("Dep 2")]
        public string DepressionTwo { get; set; }

        [DisplayName("Score")]
        public int DepressionTwoSC { get; set; }

        [DisplayName("Anger")]
        public string AngerOne { get; set; }

        [DisplayName("Score")]
        public int AngerOneSC { get; set; }

        [DisplayName("Mania")]
        public string ManiaOne { get; set; }

        [DisplayName("Score")]
        public int ManiaOneSC { get; set; }

        [DisplayName("Mania")]
        public string ManiaTwo { get; set; }

        [DisplayName("Score")]
        public int ManiaTwoSC { get; set; }

        [DisplayName("Anx 1")]
        public string AnxietyOne { get; set; }

        [DisplayName("Score")]
        public int AnxietyOneSC { get; set; }

        [DisplayName("Anx 2")]
        public string AnxietyTwo { get; set; }

        [DisplayName("Score")]
        public int AnxietyTwoSC { get; set; }

        [DisplayName("Anx 3")]
        public string AnxietyThree { get; set; }

        [DisplayName("Score")]
        public int AnxietyThreeSC { get; set; }
        
        [DisplayName("Somatic")]
        public string SomaticOne { get; set; }

        [DisplayName("Score")]
        public int SomaticOneSC { get; set; }

        [DisplayName("Somatic")]
        public string SomaticTwo { get; set; }

        [DisplayName("Score")]
        public int SomaticTwoSC { get; set; }
        
        [DisplayName("Suicidal")]
        public string SuicidalOne { get; set; }

        [DisplayName("Score")]
        public int SuicidalOneSC { get; set; }

        [DisplayName("Psych 1")]
        public string PsychosisOne { get; set; }

        [DisplayName("Score")]
        public int PsychosisOneSC { get; set; }

        [DisplayName("Psych 2")]
        public string PsychosisTwo { get; set; }

        [DisplayName("Score")]
        public int PsychosisTwoSC { get; set; }

        [DisplayName("Sleep")]
        public string SleepOne { get; set; }

        [DisplayName("Score")]
        public int SleepOneSC { get; set; }

        [DisplayName("Memory")]
        public string MemoryOne { get; set; }

        [DisplayName("Score")]
        public int MemoryOneSC { get; set; }
        
        [DisplayName("Behav 1")]
        public string BehaviourOne { get; set; }

        [DisplayName("Score")]
        public int BehaviourOneSC { get; set; }

        [DisplayName("Behav 2")]
        public string BehaviourTwo { get; set; }

        [DisplayName("Score")]
        public int BehaviourTwoSC { get; set; }
        
        [DisplayName("Dissociation")]
        public string DissociationOne { get; set; }

        [DisplayName("Score")]
        public int DissociationOneSC { get; set; }
        
        [DisplayName("Personality 1")]
        public string PersonalityOne { get; set; }

        [DisplayName("Score")]
        public int PersonalityOneSC { get; set; }

        [DisplayName("Personality 2")]
        public string PersonalityTwo { get; set; }

        [DisplayName("Score")]
        public int PersonalityTwoSC { get; set; }

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

        public int? RiskID { get; set; } 
        public int? DSM5ScoreID { get; set; } 
    }
}