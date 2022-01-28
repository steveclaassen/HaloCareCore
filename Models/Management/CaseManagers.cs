using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models.Management
{
    public class CaseManagers
    {
        [Key]
        [DisplayName("Case manager #")]
        public string caseManagerNo { get; set; }

        [Required]
        [DisplayName("First name")]
        public string caseManagerName { get; set; }

        [DisplayName("First name")]
        public string caseManagerName_UC
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(caseManagerName.ToUpper().Substring(0, 1) + caseManagerName.ToLower().Substring(1));
            }
        }

        [DisplayName("Last name")]
        public string caseManagerSurname { get; set; }

        [DisplayName("Last name")]
        public string caseManagerSurname_UC
        {
            get
            {
                if (String.IsNullOrEmpty(caseManagerSurname))
                {
                    return caseManagerSurname;
                }
                else
                {
                    return caseManagerSurname.ToUpper().Substring(0, 1) + caseManagerSurname.ToLower().Substring(1);
                }
            }
        }

        [DisplayName("Case manager")]
        public string cmFullName
        {
            get
            {
                return caseManagerName_UC + " " + caseManagerSurname_UC;
            }
        }

        [DisplayName("Initials")]
        public string cmInitial
        {
            get
            {
                return caseManagerName.ToUpper().Substring(0, 1) + caseManagerSurname.ToUpper().Substring(0, 1);
            }
        }

        [Required]
        [DisplayName("Ext.")]
        public string extension { get; set; }

        [Required]
        [DisplayName("Work")]
        public string workNo { get; set; }

        [Required]
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
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

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}