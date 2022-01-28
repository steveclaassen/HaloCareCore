using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Admin
{
    public class Log
    {
        [Key]
        public Guid LogID { get; set; }

        [Required]
        [DisplayName("Event type")]
        public string EventType { get; set; }

        [Required]
        [DisplayName("Table name")]
        public string TableName { get; set; }

        [DisplayName("Action ID")]
        public string ActionID { get; set; }

        [Required]
        [DisplayName("Record ID")]
        public string RecordID { get; set; }

        [Required]
        [DisplayName("Column name")]
        public string ColumnName { get; set; }

        [DisplayName("Original value")]
        public string OriginalValue { get; set; }
        
        [DisplayName("New value")]
        public string NewValue { get; set; }
        
        [Required]
        [DisplayName("Created by")]
        public string Created_by { get; set; }
        
        [Required]
        [DisplayName("Created date")]
        public DateTime Created_date { get; set; }
    }
}