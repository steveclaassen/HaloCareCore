using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class paediatric
    {
        [Key]
        public int paediatricID { set; get; }

        public Guid dependentID { get; set; }

        [Required]
        [DisplayName("Current weight")]
        public string currentWeight { get; set; }

        [Required]
        [DisplayName("Rx dose in-line with weight")]
        public string RxDoseIn_lineWithWeight { get; set; }

        [Required]
        [DisplayName("Syrup vs tablets- able to swallow ")]
        public string syrupVsTablets_ableToSwallow  { get; set; }

        [Required]
        [DisplayName("Status awareness where appropriate")]
        public string statusAwareness  { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }
        public Guid programId { get; set; }
        public bool assignementSent { set; get; }

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
    }
}