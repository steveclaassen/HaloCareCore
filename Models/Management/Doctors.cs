using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Globalization;


namespace HaloCareCore.Models.Management
{
    public class Doctors
    {
        [Key]
        [Required]
        public Guid doctorID { get; set; }

        [DisplayName("Title")]
        public string title { get; set; }
                
        [DisplayName("First name")]
        public string drFirstName { get; set; }

        [DisplayName("Doctor full names")]
        public string drLastName { get; set; }

        [DisplayName("Gender")]
        public string sex { get; set; }

        [DisplayName("Language")]
        public string language { get; set; }

        [DisplayName("ID #")]
        public string idNo { get; set; }

        [DisplayName("Position")]
        public string position { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Practice #")]
        public string practiceNo { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

        public string drName
        {
            get
            {
                if (!string.IsNullOrEmpty(drFirstName) && !string.IsNullOrEmpty(drLastName))
                {
                    return drFirstName.Substring(0, 1) + " " + lastName_UL;
                }
                else if (!string.IsNullOrEmpty(drLastName))
                {
                    return lastName_UL;
                }
                else
                {
                    return "";
                }
            }
        }

        [DisplayName("Title")]
        public string title_UL
        {
            get
            {
                if (!string.IsNullOrEmpty(title))
                {
                    return title.ToUpper().Substring(0, 1) + title.ToLower().Substring(1);
                }
                else
                {
                    return "";
                }
            }
        }

        [DisplayName("Last name")]
        public string lastName_UL
        {
            get
            {
                if (!string.IsNullOrEmpty(drLastName))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(drLastName.ToUpper().Substring(0, 1) + drLastName.ToLower().Substring(1));
                }
                else
                {
                    return "";
                }
            }
        }

    }
}