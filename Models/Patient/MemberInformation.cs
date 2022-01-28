using HaloCareCore.Models.Validation;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Patient
{
    public class MemberInformation
    {
        [Key]
        public Guid MemberInformationID { get; set; }

        public Guid? memberID { get; set; }

        public Guid dependantID { get; set; }
        
        public Guid? contactID { get; set; }

        public Guid? addressID { get; set; }

        public Guid? programID { get; set; }

        public string caseManagerID { get; set; }

        public Guid? doctorID { get; set; }

        [DisplayName("Allow text communication")]
        public bool AllowText { get; set; }

        public Member Members { get; set; }
        public Dependant Dependents { get; set; }
        public Address Address { get; set; }
        public Contact Contacts { get; set; }
        public Programs Program { get; set; }

    }
}