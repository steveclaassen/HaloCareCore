using System;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Communications
{
    public class AssignmentItems
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public int assignmentId { get; set; }

        [Required]
        public string itemType { get; set; }

        [Required]
        public string createdBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        public string modifiedBy { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        public string documentName { get; set; }

        public int capturedId { get; set; }

        [Required]
        public bool active { get; set; }
    }
}