using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class PathologyFields
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Pathology field")]
        public string PathologyField { get; set; }

        public string DisplayName { get; set; } //HCare-815

        public string PathologyField_UC
        {
            get
            {
                if (!string.IsNullOrEmpty(PathologyField))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(PathologyField.ToUpper().Substring(0, 1) + PathologyField.ToLower().Substring(1));
                }
                else
                {
                    return PathologyField;
                }
            }
        }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        public DateTime EffectiveDate { get; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

    }
}