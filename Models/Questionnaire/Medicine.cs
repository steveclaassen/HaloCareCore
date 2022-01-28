using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Questionnaire
{
    public class Medicine
    {
        [Key]
        [DisplayName("Medicine ID")]
        public int MedicineID { get; set; }

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }
   
        [DisplayName("Adherence ")]
        public string Adherence { set; get; }
        
        [DisplayName("Change of regime")]
        public string ChangeofRegime { set; get; }
     
        [DisplayName("Insulin")]
        public string PatientsOnInsulin { set; get; }
  
        [DisplayName("Sick-day management")]
        public string Sick_day_management { set; get; }
     
        [DisplayName("Use of Statins")]
        public string UseOfStatins { set; get; }
        public bool assignementSent { set; get; }
        public Guid programId { get; set; }

       [DisplayName("Name of medication")]
       public string nameOfMedication { get; set; }
    
        [DisplayName("Time & dosage of medication")]
        public string TimeAndDosageOfMedication { get; set; }

       
        [DisplayName("Meds interrupted")]
        public string medsInterrupted { get; set; }
       
        [DisplayName("Change of regime")]
        public string changeOfRegimeHIV { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("Change of regime")]
        public string MentalChangeOfRegime { get; set; }

        [DisplayName("Side effects")]
        public string SideEffects { get; set; }

        [DisplayName("Drug/Substance abuse")]
        public string drugOrsubstanceAbuseoperty { get; set; }
        //[NotMapped]
        public string[] PatientsOnInsulin_Concat
        {
            get
            {
                if (PatientsOnInsulin != null)
                {
                    return PatientsOnInsulin.Split(',');
                }
                else
                {
                    return PatientsOnInsulin_Concat;
                }
            }
            set
            {
                PatientsOnInsulin = string.Join(",", value);
            }
        }
        //[NotMapped]
        public string[] changeOfRegimeHIV_Concat
        {
            get
            {
                if (changeOfRegimeHIV != null)
                {
                    return changeOfRegimeHIV.Split(',');
                }
                else
                {
                    return changeOfRegimeHIV_Concat;
                }
            }
            set
            {
                changeOfRegimeHIV = string.Join(",", value);
            }
        }
        
        //[NotMapped]
        public string[] changeOfRegimeMental_Concat
        {
            get
            {
                if (MentalChangeOfRegime != null)
                {
                    return MentalChangeOfRegime.Split(',');
                }
                else
                {
                    return changeOfRegimeMental_Concat;
                }
            }
            set
            {
                MentalChangeOfRegime = string.Join(",", value);
            }
        }

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