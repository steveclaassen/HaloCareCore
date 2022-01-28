using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Questionnaire
{
    public class NutritionAndLifestyle
    {
        
        [DisplayName("Nutrition And Lifestyle ID")]
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int nutritionAndLifestyleID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

       
        [DisplayName("Diet history")]
        public string dietHistory { get; set; }
      
        [DisplayName("Healthy eating habits")]
        public string healthyEatingHabits { get; set; }
       
        [DisplayName("Exercise & activity")]
        public string exerciseAndActivity { get; set; }
       
        [DisplayName("Weight management")]
        public string weightManagementAddressed { get; set; }

        [DisplayName("Sleep pattern")]
        public string SleepPattern { get; set; }
        public Guid programId { get; set; }

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