using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class GeneralReportingViewModel
    {
        public Guid dependantID { get; set; }

        [DisplayName("Member #")]
        public string membershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("Medical scheme")]
        public string medicalAidCode { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Member surname")]
        public string memberSurname { get; set; }

        [DisplayName("ID #")]
        public string idNumber { get; set; }

        [DisplayName("Gender")]
        public string Gender { get; set; }

        [DisplayName("Option")]
        public string schemeOption { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("Program start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? programStartDate { get; set; }

        [DisplayName("Member status")]
        public string memberStatus { get; set; }

        [DisplayName("Status date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? statusStartDate { get; set; }
    }
}