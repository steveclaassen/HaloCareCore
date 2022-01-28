using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class PathologyIndex
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LabName { get; set; }

        [Required]
        public string LabReferenceNumber { get; set; }

        [Required]
        public string createdBy { get; set; }

        [Required]
        public DateTime createdDate { get; set; }

        [Required]
        public Guid dependantId { get; set; }

        [Required]
        public DateTime effectiveDate { get; set; }

        public int sentToCL { get; set; }


    }
}