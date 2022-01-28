using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class PatientSummaryView
    {
        [DisplayName("Next pathology")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? nextPathologyDate { get; set; }

        [DisplayName("Last pathology")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? lastPathologyDate { get; set; }

        [DisplayName("Pathology behind")]
        public int pathologyBehind { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? scriptEffective { get; set; }

        public AssignmentsView assignments { get; set; }
        public List<Pathology> pathologies { get; set; }
        public List<ScriptViewModel> ScriptItems { get; set; }
    }
}