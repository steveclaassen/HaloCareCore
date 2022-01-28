using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class SystemLogResultsVM
    {
        public Guid LogID { get; set; }

        [DisplayName("Event type")]
        public string EventType { get; set; }

        [DisplayName("Record ID")]
        public string RecordID { get; set; }

        [DisplayName("Table name")]
        public string TableName { get; set; }

        [DisplayName("Column name")]
        public string ColumnName { get; set; }

        [DisplayName("CurrentVAlue")]
        public string CurrentValue { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }
     
    }
}
