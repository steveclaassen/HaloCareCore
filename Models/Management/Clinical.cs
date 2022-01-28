using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class Clinical
    {
        [Key]
        [Required]
        public int id { get; set; }

        public Guid? questionnaireID { get; set; }

        //[Required] //HCare-671
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        [DisplayName("Weight in kg")]
        public decimal weight { get; set; }

        [DisplayName("Height in meters")]
        [Range(0, 2.5, ErrorMessage = "Height must be between 0 & 2.5 meters")]
        public decimal height { get; set; }

        [DisplayName("BMI")]
        public decimal bmi { get; set; }

        [DisplayName("Body surface area")]
        public decimal bodyServiceArea { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Comment")]
        public string clinicalComment { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        public bool sentToCl { get; set; }
    }
}