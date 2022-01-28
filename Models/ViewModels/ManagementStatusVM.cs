using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ManagementStatusVM
    {
        public int id { get; set; }

        public Guid dependantId { get; set; }

        [DisplayName("Management status code")]
        public string managementStatusCode { get; set; }

        [DisplayName("Management status")]
        public string managementStatusName { get; set; }

        [DisplayName("Code status")]
        public string codeStatus { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? endDate { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? effectiveDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        public bool sentToCl { get; set; }

        public ManagementStatus managementStatus { get; set; }
        public List<ManagementStatus> managementStatuses { get; set; }
        public ManagementStatus_DeactivatedReasons msReason { get; set; }
        public List<ManagementStatus_DeactivatedReasons> msReasons { get; set; }
        public List<ManagementStatusVM> ManagementStatusVMs { get; set; }
        public string programCode { get; set; }

    }
}
