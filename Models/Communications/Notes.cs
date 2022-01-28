using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class Notes
    {
        [Key]
        [Required]
        public int noteID { get; set; }

        [Required]
        public Guid dependentID { get; set; }

        [Required]
        [DisplayName("Note type")]
        public string noteType { get; set; }

        [Required]
        [DisplayName("Note")]
        public string note { get; set; }
        public string importNotesSubjet { set; get; }

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

        [Required]
        [DisplayName("Date of note")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Expiry date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? expiryDate { get; set; }

        [DisplayName("Special")]
        public bool special { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public int SentToCL { get; set; }

        public Guid? programId { get; set; }

        //HCare-1408
        [DisplayName("Prescriber pr#")]
        public string PrescriberPr { get; set; }
        [DisplayName("Prescriber Name")]
        public string PrescriberName { get; set; }
    }
}