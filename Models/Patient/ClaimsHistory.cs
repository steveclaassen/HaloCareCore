using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class ClaimsHistory
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public Guid dependentId { get; set; }

        [DisplayName("Message")]
        public string claimMessage { get; set; }

        [DisplayName("Status")]
        public string claimStatus { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Claim date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? claimDate { get; set; }

    }
}