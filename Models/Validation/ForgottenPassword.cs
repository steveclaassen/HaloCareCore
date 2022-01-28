using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ForgottenPassword
    {
        [Key]
        public int ForgottenPasswordID { get; set; }

        [Required]
        public Guid userID { get; set; }

        [Required]
        [DisplayName("Key")]
        public string key { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Expiry date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime expiryDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

    }
}