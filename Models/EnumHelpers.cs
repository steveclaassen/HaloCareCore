using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class EnumHelpers
    {
        public enum DependantType
        {
            Adult,
            Child,
            Dependant,
            Main,
            Principle,
            Spouse
        }

        public enum ContactType
        {
            Aunt,
            Brother,
            Father,
            Husband,
            Mother,
            Other,
            Partner,
            Personal,
            Self,
            Sister,
            Uncle,
            Wife
        }

        public enum Gender
        {
            Female,
            Male
        }
    }
}