using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class GeneralContactViewModel
    {
        public Guid ContactID { get; set; }

        [DisplayName("Patient")]
        public string PatientName { get; set; }

        [DisplayName("ID #")]
        public string IdNumber { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Contact name")]
        public string contactName { get; set; }

        [DisplayName("Contact number")]
        public string contactNumber { get; set; }

        [DisplayName("Email")]
        public string email { get; set; }

        [DisplayName("Mobile")]
        public string cell { get; set; }

        [DisplayName("Home")]
        public string landLine { get; set; }

        [DisplayName("Work")]
        public string workNo { get; set; }

        [DisplayName("Fax")]
        public string fax { get; set; }

        [DisplayName("Practice #")]
        public string drPracticeNo { get; set; }

        [DisplayName("Doctor contact name")]
        public string drContactName { get; set; }

        [DisplayName("Doctor contact #")]
        public string drContactNumber { get; set; }

        [DisplayName("Doctor Mobile")]
        public string drcell { get; set; }
    }
}