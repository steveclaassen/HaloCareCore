using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Management
{
    public class PatientStagingHistory
    {
        [Key]
        public int id { get; set; }
        public Guid dependantId { get; set; }
        public string stageCode { get; set; }
        public decimal? cd4Count { get; set; }
        public int? cd4Percentage { get; set; }
        public decimal? viralLoad { get; set; }

        [DisplayName("Effective date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)] 
        public DateTime effectiveDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }
        
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)] 
        public DateTime? createdDate { get; set; }
        [DisplayName("Modified By")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }
        [DisplayName("Active")]
        public bool active { get; set; }
        public string comment { get; set; }

        [NotMapped]
        public List<string> stages { get; set; }

    }
}
