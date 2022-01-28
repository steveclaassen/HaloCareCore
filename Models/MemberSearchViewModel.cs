using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;

namespace HaloCareCore.Models
{
    public class MemberSearchViewModel
    {
        public Guid MedicalAidID { get; set; }
        public Guid programID { get; set; }
        public Guid memberID { get; set; }
        public string membershipNo { get; set; }
        public Guid DependantID { get; set; }
        
        [DisplayName("Dependant code")]
        public string dependentCode { get; set; }
        [DisplayName("Initials")]
        public string initials { get; set; }
        [DisplayName("Initials")]
        public string initials_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(initials))
                {
                    return initials.ToUpper();
                }
                else
                {
                    return initials;
                }
            }
        }
        [DisplayName("First name")]
        public string firstName { get; set; }
        [DisplayName("Last name")]
        public string lastName { get; set; }
        [DisplayName("First name")]
        public string firstName_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(firstName))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName.ToUpper().Substring(0, 1) + firstName.ToLower().Substring(1));
                }
                else
                {
                    return firstName;
                }
            }
        }
        [DisplayName("Last name")]
        public string lastName_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(lastName))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName.ToUpper().Substring(0, 1) + lastName.ToLower().Substring(1));
                }
                else
                {
                    return lastName;
                }
            }
        }
        [DisplayName("ID/Auth #")]
        public string idNumber { get; set; }

        [DisplayName("Birth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime birthDate { get; set; }
        [DisplayName("Gender")]
        public string sex { get; set; }
      
        [DisplayName("Scheme")]
        public string scheme { get; set; }

        [DisplayName("Status")]
        public string patientStatus { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }

        public string searchProperty { set; get; }

        public Users User { get; set; } //hcare-1483
    }
}