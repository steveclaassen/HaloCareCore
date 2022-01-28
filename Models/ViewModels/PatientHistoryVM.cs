using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class PatientHistoryVM
    {
        //Hcare-1195
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("ICD-10 code added")]
        public string icdAdded { get; set; }

        [DisplayName("ICD-10 code updated")]
        public string icdUpdated { get; set; }

        [DisplayName("ICD-10 code removed")]
        public string icdRemoved { get; set; }

    }
}