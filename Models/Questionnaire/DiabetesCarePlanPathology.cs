using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class DiabetesCarePlanPathology
    {
        [Key]
        [DisplayName("Pathology ID")]
        public int PathologyID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [Required]
        [DisplayName("Average glucose readings")]
        public string Average_glucose_readings { get; set; }
        [Required]
        [DisplayName("monitoring and management")]
        public string Hypoglycemia { get; set; }
        [Required]
        [DisplayName("Home glucose testing ")]
        public string HomeGlucoseTesting { get; set; }
        [Required]
        [DisplayName("Glucose diary")]
        public string Glucose_diary { get; set; }
        [Required]
        [DisplayName("Latest HbA1C ")]
        public string LatestHbA1C  { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }
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