using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models
{
    public class CompactOpenAssignments
    {
        [DisplayName("Dependant code")]
        public string dependantCode { get; set; }

        [DisplayName("Dependant ID")]
        public string dependantID { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Patient name")]
        public string patientName { get; set; }

        [DisplayName("Patient name")]
        public string patientName_UC
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patientName.ToUpper().Substring(0, 1) + patientName.ToLower().Substring(1));
            }
        }

        [DisplayName("Scheme")]
        public string medicalScheme { get; set; }

        [DisplayName("Option")]
        public string option { get; set; }

        [DisplayName("Patient status")]
        public string patientStatus { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("Assignment count")]
        public int assignmentCount { get; set; }

        public int taskClosedCount { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid programID { get; set; }
    }
}