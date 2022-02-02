using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaloCareCore.Models.Patient
{
    public class Dependant
    {
        [Key]
        public Guid DependantID { get; set; }

        public Guid memberID { get; set; }

        [Required]
        [StringLength(2, ErrorMessage = "Dep code restricted to 2 characters eg. 00")]
        [DisplayName("Dependant code")]
        public string dependentCode { get; set; }

        [Required(AllowEmptyStrings =true)]
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

        [Required]
        [DisplayName("First name")]
        public string firstName { get; set; }

        [Required]
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

        [DisplayName("Full name")]
        public string fullName_ul
        {
            get
            {
                return firstName_UC + " " + lastName_UC;
            }
            set { fullNameUC = value; }//HCare-1088

        }
        public int MyProperty { get; set; }

        [Required]
        [DisplayName("ID/Auth #")]
        public string idNumber { get; set; }

        [Required]
        [DisplayName("Birth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime birthDate { get; set; }

        [Required]
        [DisplayName("Gender")]
        public string sex { get; set; }

        [DisplayName("Demographic")]
        public string demographic { get; set; }

        [Required]
        [DisplayName("Principle member")]
        public bool isPrinciple { get; set; }

        [Required]
        [DisplayName("SA citizen")]
        public bool isSACitizen { get; set; }

        [Required]
        [DisplayName("Language")]
        public string languageCode { get; set; }

        [DisplayName("Dispensing provider")]
        public string dispensingProvider { get; set; }

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

        [DisplayName("Dependant type")]
        public string dependentType { get; set; }

        [DisplayName("Origination")]
        public string originationId { get; set; }

        [DisplayName("ICD-10 code")] //HCare-958
        public string icd10Code { get; set; }

        [DisplayName("Title")]
        public string title { get; set; } 

        [DisplayName("Title")]
        public string Title_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(title))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToUpper().Substring(0, 1) + title.ToLower().Substring(1));
                }
                else
                {
                    return title;
                }
            }
            set {  } //HCare-1088
            
        }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [Required]
        [DisplayName("SMS OPT-OUT")]
        public bool optOut { get; set; }

        [DisplayName("Age")]
        public string dependantAge
        {
            get
            {
                DateTime dateOfBirth = new DateTime();

                int currentYear, currentMonth, birthMonth, birthYear, years, months;

                dateOfBirth = Convert.ToDateTime(birthDate);
                currentYear = Convert.ToInt32(DateTime.Now.Year);
                currentMonth = Convert.ToInt32(DateTime.Now.Month);
                birthYear = Convert.ToInt32(dateOfBirth.Year);
                birthMonth = Convert.ToInt32(dateOfBirth.Month);

                years = currentYear - birthYear;

                if ((currentMonth - birthMonth > 0))
                {
                    months = Convert.ToInt32(currentMonth - birthMonth);
                }
                else
                {
                    years = years - 1;
                    months = Convert.ToInt32((12 - birthMonth) + currentMonth);
                }

                return years.ToString() + "y " + months.ToString() + "m";

            }
        }

        [DisplayName("Previous MembershipNo")]
        public string prevMembershipNo { get; set; }

        [NotMapped]
        public string fullNameUC { set; get; } //HCare-1088_test
    }


}