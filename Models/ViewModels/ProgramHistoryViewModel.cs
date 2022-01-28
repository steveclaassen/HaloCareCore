using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class ProgramHistoryViewModel
    {
        public List<PatientProgramHistory> Histories { get; set; }
        public List<PatientProgramSubHistory> PatientProgramSubHistories { get; set; }
        public List<PatientHistoryVM> SubHistories { get; set; }//Hcare-1195
        public List<PatientDiagnosis> MentalHealthDiagnoses { get; set; } //hcare-1203 //hcare-1312:correction
        public List<MHDiagnosisVM> MHDiagnoses { get; set; } //hcare-1203
        public List<PatientProgramVM> programHistories { get; set; } //hcare-1203
        public List<PatientHistoryVM> diagnosisHistories { get; set; } //hcare-1203
    }
}