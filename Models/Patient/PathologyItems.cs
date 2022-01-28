using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class PathologyItems
    {
        [Key]
        [Required]
        public int itemId { get; set; }

        [Required]
        public int pathologyId { get; set; }

        [Required]
        public string test { get; set; }

        [Required]
        public decimal result { get; set; }

        [Required]
        public string createdBy { get; set; }
        
        [Required]
        public DateTime createdDate { get; set; }

        [Required]
        public DateTime effectiveDate { get; set; }

        [Required]
        public bool currentValue { get; set; }

        [Required]
        public Guid dependantId { get; set; }

        [Required]
        public bool sentToCl { get; set; }

    }
}