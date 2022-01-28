using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Validation
{
    public class EnrolmentStepsMonitor
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public Guid dependantId { get; set; }

        [DisplayName("Prescription")]
        public bool hasScript { get; set; }

        [DisplayName("Captured")]
        public bool scriptCaptured { get; set; }

        [DisplayName("Pathology")]
        public bool hasPathology { get; set; }

        [DisplayName("Captured")]
        public bool pathologyCaptured { get; set; }

        [DisplayName("Clinical history")]
        public bool hasClinical { get; set; }

        [DisplayName("Captured")]
        public bool clinicalCaptured { get; set; }

        [DisplayName("Demographic")]
        public bool hasDemographic { get; set; }

        [DisplayName("Captured")]
        public bool demographicCaptured { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        public DateTime createdDate { get; set; }

    }
}