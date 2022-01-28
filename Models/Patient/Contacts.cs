using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class Contact
    {
        [Key]
        public Guid ContactID { get; set; }

        [DisplayName("Contact")]
        public string contactName { get; set; }

        [DisplayName("Cell #")]
        public string cell { get; set; }

        [DisplayName("Home #")]
        public string landLine { get; set; }

        [DisplayName("Work #")]
        public string workNo { get; set; }

        [DisplayName("Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [DisplayName("Fax #")]
        public string fax { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Preferred")]
        public string preferredMethodOfContact { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Additional Cell #")]
        public string ISeriesCell { get; set; }

        [DisplayName("Additional Tel #")]
        public string ISeriesLandLine { get; set; }

        [DisplayName("Additional Work #")]
        public string ISeriesWorkNo { get; set; }

        [DisplayName("Additional Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string ISeriesEmail { get; set; }
    }
}