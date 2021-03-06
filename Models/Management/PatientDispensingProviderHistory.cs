using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class PatientDispensingProviderHistory
    {
        [Required]
        public int id { get; set; }

        public Guid dependantId { get; set; }

        [DisplayName("Dispensing ID")]
        public int dispensingId { get; set; }

        [DisplayName("Dispensing name")]
        public string dispensingName { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

    }
}