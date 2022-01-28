using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ClinicalHistoryViewModel
    {
        public ClinicalHistoryQuestionaire questionnaire { get; set; }
        public MedicationHistory medication { get; set; }
        public Clinical clinical { get; set; }
        public Allergies allergy { get; set; }

        public List<MedicationHistory> medications { get; set; }
        public List<Clinical> clinicals { get; set; }
        public List<Allergies> allergies { get; set; } 
    }
}