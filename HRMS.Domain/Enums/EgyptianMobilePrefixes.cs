using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    // بادئات شبكات المحمول فى مصر - المصدر الوحيد لهذه القيم فى المشروع كله
    public static class EgyptianMobilePrefixes
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>
        {
            "010", "011", "012", "015"
        };
    }
}
