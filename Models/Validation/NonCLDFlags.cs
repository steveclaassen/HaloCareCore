using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class NonCLDFlags
    {
        [Key]
        [Required]
        [DisplayName("Id")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Flag Code")]
        public string flagCode { get; set; }

        [Required]
        [DisplayName("Program")]
        public string programCode { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        
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
    }
}