using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    public static class GenderExtensions
    {
        public static string ToArabicName(this Gender gender) => gender switch
        {
            Gender.Male => "ذكر",
            Gender.Female => "أنثى",
            _ => gender.ToString()
        };
    }
}
