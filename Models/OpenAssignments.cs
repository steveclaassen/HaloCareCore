using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;



namespace HaloCareCore.Models
{
    public class OpenAssignments
    {
        [DisplayName("Assignment ID")]
        public int assignmentID { get; set; }

        [DisplayName("Dep code")]
        public string dependantCode { get; set; }

        [DisplayName("Dependant ID")]
        public string dependantID { get; set; }

        [DisplayName("Assignment type")]
        public string assignmentType { get; set; }

        [DisplayName("Item type")]
        public string assignmentitemType { get; set; }

        [DisplayName("Member number")]
        public string membershipNumber { get; set; }

        [DisplayName("ID/Auth #")]
        public string idNumber { get; set; } //HCare-941

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

        [DisplayName("Lab")]
        public string labName { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? assignmentEffective { get; set; }

        public bool Active { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("Assignment status")]
        public string assignmentStatus { get; set; }

        public string assignmentAge { get; set; }

        public string elapsedTime { get; set; }


        //[DisplayName("Time open")]
        //[DisplayFormat(DataFormatString = "{0:d\\d}" + " " + "{0:h\\h}" + " " + "{0:m\\m}", ApplyFormatInEditMode = true)]
        //public TimeSpan Time
        //{
        //    get
        //    {
        //        DateTime now = DateTime.Now;

        //        var startTime = Convert.ToDateTime(createdDate);
        //        var endTime = Convert.ToDateTime(now);
        //        var difference = endTime - startTime;

        //        return difference;

        //    }
        //}

        public int taskClosedCount { get; set; }
        public string itemType { get; set; }

        //[DisplayName("Created date")]
        //[DataType(DataType.DateTime)]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime? createdDate { get; set; }

        [DisplayName("SchemeID")]
        public Guid MedicalAidID { get; set; }

        public Guid programID { get; set; }

        public Guid? assignmentProgramID { get; set; }

    }
}