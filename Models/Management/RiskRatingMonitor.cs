using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Management
{
    public class RiskRatingMonitor
    {
        public Guid Id { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        public List<RiskRatingTypes> RiskRatingTypes { get; set; }

        [DisplayName("Risk rating from")]
        public string From { get; set; }

        [DisplayName("Risk rating to")]
        public string To { get; set; }

        [DisplayName("Percentage")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")]
        [Range(0, 9999999999999999.99)]
        public decimal Percentage { get; set; }

        public List<Programs> Programs { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

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
