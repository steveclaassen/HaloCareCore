using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Communications
{
    public class AttachmentTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("Attachment name")]
        public string AttachmentName { get; set; }
        
        [DisplayName("Attachment extension")]
        public string AttachmentExtension { get; set; }

        [DisplayName("Attachment type")]
        public string AttachmentType { get; set; }
        
        [DisplayName("Root")]
        public string Root { get; set; }
        
        [DisplayName("File name")]
        public string FileName { get; set; }

        public List<Programs> Programs { get; set; }
        
        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}
