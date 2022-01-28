using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class CarePlanPathology
    {
        [Key]
        [DisplayName("Pathology ID")]
        public int PathologyID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

       
        [DisplayName("Avg. glucose")]
        public string Average_glucose_readings { get; set; }
       
        [DisplayName("Hypoglycemia ")]
        public string Hypoglycemia { get; set; }
       
        [DisplayName("Home glucose testing ")]
        public string HomeGlucoseTesting { get; set; }
        
        [DisplayName("Glucose diary")]
        public string Glucose_diary { get; set; }
       
        [DisplayName("Latest HbA1C ")]
        public string LatestHbA1C  { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }
        public bool assignementSent { set; get; }
        public Guid programId { get; set; }

      
        //[DisplayName("viral loads/ CD4 count and % /other")]
        [DisplayName("HIV pathology")]
        public string viralLoads { get; set; }

       
        [DisplayName("Viral load abnormalities")]
        public string viralLoadAbnormalities { get; set; }


        [DisplayName("CD4 count")]
        public string CD4count  { get; set; }

        //[DisplayName("Other related result abnormalities discussed")]
        [DisplayName("Other abnormalities")]
        public string otherRelatedResult { get; set; }

        [DisplayName("Monitor kidney function")]
        public string monitorKidneyFunction { get; set; }

     
        //[DisplayName("Other: STI/Papsmear/Condom use")]
        [DisplayName("Other")]
        public string other { get; set; }

        [DisplayName("Latest blood results discussed")]
        public string LatestBloodResultsDiscussed { get; set; }

        [DisplayName("Pathology outstanding Co-Morbidities")]
        public string PathologyOutstandingCo_Morbidities { get; set; }

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