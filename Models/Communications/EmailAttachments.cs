using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    // HCare-860
    public class EmailAttachments
    {
        [Key]
        public int FileId { get; set; }
        
        public string FilePath { get; set; }

        public int EmailID { get; set; }
        public virtual Emails Email { get; set; }
    }
}