using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class ContactInformationVM
    {
        public Guid dependantID { get; set; }

        [DisplayName("Title")]
        public string memberTitle { get; set; }

        [DisplayName("Member name")]
        public string memberName { get; set; }

        [DisplayName("Language")]
        public string language { get; set; }

        [DisplayName("Mobile")]
        public string memberMobile { get; set; }

        [DisplayName("Home #")]
        public string landLine { get; set; }

        [DisplayName("Work #")]
        public string workNo { get; set; }

        [DisplayName("Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [DisplayName("Fax #")]
        public string fax { get; set; }

        [DisplayName("Alternate contact")]
        public string alternateContact { get; set; }

        [DisplayName("Alternate mobile")]
        public string alternateMobile { get; set; }
    }
}
