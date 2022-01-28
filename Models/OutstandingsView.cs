using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class OutstandingsView
    {
        public Guid dependantID { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dep code")]
        public string dependantCode { get; set; }

        [DisplayName("Member name")]
        public string firstName { get; set; }

        [DisplayName("Member surname ")]
        public string lastName { get; set; }

        [DisplayName("Id/Passport #")]
        public string idNumber { get; set; }

        [DisplayName("Scheme")]
        public string schemeName { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Time overdue")]
        public string overdue { get; set; }
        
        [DisplayName("Option")]
        public string schemeOption { get; set; }

        [DisplayName("Program")]
        public string registeredProgram { get; set; }

        [DisplayName("BHF")]
        public string TreatingDrBHF { get; set; }

        [DisplayName("Doctor name")]
        public string TreatingDrName { get; set; }

        [DisplayName("Doctor email")]
        public string drEmail { get; set; }

        [DisplayName("Lab")]
        public string lastLabName { get; set; }

        [DisplayName("Script outstanding")]
        public string monthsOutstanding { get; set; }

        [DisplayName("Expired date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? expiredDate { get; set; }
    }
}