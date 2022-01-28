using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class PatientStagingHistoryViewModel
    {
        public int id { set; get; }
        public Guid dependentID { get; set; }    
        public string HivStaging { get; set; }
        public DateTime? effectiveDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { set; get; }    
        public string ModidiedBy { set; get; }
        public bool StagingActive { set; get; } = true;
        public bool RiskingActive { set; get; } = true;
        public string Reason { get; set; }
        public int StagingFk { get; set; } //Foreign key
        public PatientStagingHistory patientStagingHistory { get; set; }
        public PatientRiskRatingHistory patientRiskRatingHistory { get; set; }
    }
}