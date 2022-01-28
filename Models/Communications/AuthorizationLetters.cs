using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class AuthorizationLetters
    {
        [Key]
        [Required]
        [DisplayName("Auth letter ID")]
        public string AuthLetterID { get; set; }

        public int AuthLinkID { get; set; }

        [DisplayName("Script #")]
        public int scriptNo { get; set; }

        public Guid dependantID { get; set; }

        public int? assignmentID { get; set; }

        [DisplayName("Sent via")]
        public string sentVia { get; set; }

        [DisplayName("Sent to")]
        public string sentTo { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}