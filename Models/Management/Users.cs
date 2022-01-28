using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models.Management
{
    public class Users
    {
        [Key]
        [Required]
        public Guid userID { get; set; }

        [Required]
        [DisplayName("First name")]
        public string Firstname { get; set; }

        [DisplayName("First name")]
        public string userFirstName_UC
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Firstname.ToUpper().Substring(0, 1) + Firstname.ToLower().Substring(1));
            }
        }

        [Required]
        [DisplayName("Last name")]
        public string Lastname { get; set; }

        [DisplayName("Last name")]
        public string userLastName_UC
        {
            get
            {
                return Lastname.ToUpper().Substring(0, 1) + Lastname.ToLower().Substring(1);
            }
        }

        [DisplayName("Initials")]
        public string userInitial
        {
            get
            {
                return Firstname.ToUpper().Substring(0, 1) + Lastname.ToUpper().Substring(0, 1);
            }
        }

        [DisplayName("Full name")]
        public string userFullName
        {
            get
            {
                return userFirstName_UC + " " + userLastName_UC;
            }
        }


        [Required]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        [DisplayName("Email")]
        public string email { get; set; }

        [Required]
        [DisplayName("Username")]
        public string username { get; set; }

        [Required]
        [DisplayName("Password")]
        public string password { get; set; }

        [Required]
        [DisplayName("Salt")]
        public string salt { get; set; }

        [Required]
        [DisplayName("Created")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Created")]
        public string created
        {
            get
            {
                return createdBy.ToUpper().Substring(0, 1);
            }
        }

        [DisplayName("Active")]
        public bool Active { get; set; }
        public Guid accountId { get; set; }

        [DisplayName("Search management")]
        public bool SearchManagement { get; set; } //hcare-1289

        //[DisplayName("Bulk email")]
        //public bool BulkEmail { get; set; } //hcare-1483

    }
}