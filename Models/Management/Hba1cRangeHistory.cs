using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Management
{
    //Hcare-1241
    public class Hba1cRangeHistory
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public Guid dependantId { get; set; }

        //[Required] //HCare-671
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }


        [DisplayName("Target Outcome")]
        public string targetOutcome { get; set; }

        [DisplayName("Hba1c")]
        public decimal hba1c { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("hba1c Effective Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? hba1cEffectiveDate { get; set; }

    }
}