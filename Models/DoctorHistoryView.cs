using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class DoctorHistoryView
    {
        public int id { get; set; }

        public Guid dependantId { get; set; }

        public Guid doctorId { get; set; }

        [DisplayName("Doctor name")]
        public string doctorName { get; set; }

        [DisplayName("Work number")]
        public string tel { get; set; }

        [DisplayName("Mobile")]
        public string cell { get; set; }

        [DisplayName("Email")]
        public string email { get; set; }

        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }
        public Guid? ProgramId { set; get; } //HCare-1386
    }
}