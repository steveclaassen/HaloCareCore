using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AdvancedSearchView
    {
        [DisplayName("Program code")]
        public string programCode { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }

        [DisplayName("Medical scheme ID")]
        public string medAidId { get; set; }

        [DisplayName("Case manager")]
        public string caseManager { get; set; }

        [DisplayName("Gender")]
        public string sex { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("ID #")]
        public string idNumber { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Member surname")]
        public string memberSurname { get; set; }

        [DisplayName("Mobile")]
        public string cell { get; set; }

        [DisplayName("Tel #")]
        public string telNo { get; set; }

        [DisplayName("Doctor name")]
        public string doctorName { get; set; }

        [DisplayName("Doctor surname")]
        public string doctorSurname { get; set; }

        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [DisplayName("Date from")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateFrom { get; set; }

        [DisplayName("Date to")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateTo { get; set; }

        [DisplayName("Risk rating")]
        public string riskRating { get; set; }

        [DisplayName("Doctor full names")]
        public string DoctorFullNames { set; get; } //HCare-1423


    }


}