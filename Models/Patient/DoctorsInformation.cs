using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class DoctorsInformation
    {
        [Key]
        public int Id { get; set; }
        public Guid DoctorID { get; set; }

        [DisplayName("Contact name")]
        public string ContactName { get; set; }

        [DisplayName("Mobile")]
        public string MobileNumber { get; set; }

        [DisplayName("Home")]
        public string HomeNumber { get; set; }

        [DisplayName("Work")]
        public string WorkNumber { get; set; }

        [DisplayName("Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [DisplayName("Fax #")]
        public string Fax { get; set; }

        [DisplayName("Building")]
        public string Line1 { get; set; }

        [DisplayName("Street")]
        public string Line2 { get; set; }

        [DisplayName("Suburb")]
        public string Line3 { get; set; }

        [DisplayName("City")]
        public string City { get; set; }

        [DisplayName("Province")]
        public string Province { get; set; }

        [DisplayName("Postal code")]
        public string ZipCode { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}