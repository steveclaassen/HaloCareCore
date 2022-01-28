using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class MemberImports
    {
        [Key]
        public Guid Id { get; set; }

        public Guid DependantId { get; set; }
        public string medicalAidCode { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dependant Code")]
        public string dependantCode { get; set; }

        [DisplayName("Scheme Option")]
        public string option { get; set; }

        [DisplayName("Title")]
        public string title { get; set; }

        [DisplayName("First name")]
        public string firstName { get; set; }

        [DisplayName("First name")]
        public string firstName_ul
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

        [DisplayName("Last Name")]
        public string lastName { get; set; }

        [DisplayName("Last name")]
        public string lastName_ul
        {
            get
            {
                if (!string.IsNullOrEmpty(firstName))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName.ToUpper().Substring(0, 1) + lastName.ToLower().Substring(1));
                }
                else
                {
                    return lastName;
                }
            }


        }

        [DisplayName("Full name")]
        public string fullName
        {
            get
            {
                return firstName + " " + lastName;
            }
        }

        [DisplayName("Full name")]
        public string fullName_ul
        {
            get
            {
                return firstName_ul + " " + lastName_ul;
            }
        }


        [DisplayName("Gender")]
        public string gender { get; set; }

        [DisplayName("Language")]
        public string language { get; set; }

        [DisplayName("Date of Birth")]
        public string dateOfBirth { get; set; }

        [DisplayName("ID Number")]
        public string iDNumber { get; set; }

        [DisplayName("Address Line 1")]
        public string addressLine1 { get; set; }

        [DisplayName("Address Line 2")]
        public string addressLine2 { get; set; }

        [DisplayName("City")]
        public string city { get; set; }

        [DisplayName("Postal Code")]
        public string postalCode { get; set; }

        [DisplayName("Telephone Number")]
        public string telephoneNumber { get; set; }

        [DisplayName("Cell")]
        public string cellphone { get; set; }

        [DisplayName("Email")]
        public string emailAddress { get; set; }

        [DisplayName("Option Status")]
        public string optionStatus { get; set; }

        [DisplayName("Employer Code")]
        public string employerCode { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Member Status")]
        public string memberStatus { get; set; }

        [DisplayName("Eligibility Start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? eligibilityStart { get; set; }

        [DisplayName("Eligibility End")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? eligibilityEnd { get; set; }
    }
}