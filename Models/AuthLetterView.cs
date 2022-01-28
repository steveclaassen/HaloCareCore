using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Script;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models
{
    public class AuthLetterView
    {
        public AuthorizationLetters authletter { get; set; }
        public Address address { get; set; }
        public Contact contact { get; set; }
        public Authorisations auths { get; set; }
        public MedicalAid medicalAid { get; set; }

        public List<ScriptViewModel> scriptItems { get; set; }
        public List<Attachments> attachments { get; set; }

        public Guid dependantID { get; set; }

        [DisplayName("Member #")]
        public string membershipno { get; set; }

        [DisplayName("Name")]
        public string name { get; set; }

        [DisplayName("Surname")]
        public string surname { get; set; }

        [DisplayName("Dependant code")]
        public string dependentCode { get; set; }

        [DisplayName("Birth date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime dateOfBirth { get; set; }

        [DisplayName("ID #")]
        public string IdNo { get; set; }

        [DisplayName("Gender")]
        public string gender { get; set; }

        [DisplayName("Scheme")]
        public string schemeName { get; set; }

        [DisplayName("Reference #")]
        public string referenceNo { get; set; }

        [DisplayName("Script #")]
        public int scriptNo { get; set; }

        [DisplayName("Doctor name")]
        public string drName { get; set; }

        [DisplayName("Practice #")]
        public string drPracticeNo { get; set; }

        [DisplayName("Scheme code")]
        public string schemeCode { get; set; }

        [DisplayName("Image")]
        public string medaidpic { get; set; }

        [DisplayName("Doctor email")]
        public string drEmail { get; set; }

        [DisplayName("Dispensing email")]
        public string dipensingEmail { get; set; }

        [DisplayName("Option")]
        public string medoption { get; set; }

    }
}