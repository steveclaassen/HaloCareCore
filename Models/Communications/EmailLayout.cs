using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Communications
{
    public class EmailLayout
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Layout type")]
        public string LayoutType { get; set; }

        [DisplayName("Layout size")]
        public string LayoutSize { get; set; }
        public string LayoutHeight { get; set; }
        public string LayoutWidth { get; set; }

        [DisplayName("Attachment type")]
        public string AttachmentType { get; set; }

        [DisplayName("Root")]
        public string Root { get; set; }

        [DisplayName("File name")]
        public string FileName { get; set; }

        [DisplayName("Extension")]
        public string FileExtension { get; set; }

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
