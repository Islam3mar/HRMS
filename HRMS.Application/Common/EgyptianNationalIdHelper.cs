using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Enums;

namespace HRMS.Application.Common
{
    // بيفك الرقم القومي المصري (14 رقم): [خانة القرن][YY][MM][DD][كود محافظة][تسلسل]
    public static class EgyptianNationalIdHelper
    {
        public static bool TryDecodeBirthDate(string nationalId, out DateTime birthDate, out string? error)
        {
            birthDate = default;
            error = null;

            var century = nationalId[0] switch
            {
                '2' => 1900,
                '3' => 2000,
                _ => -1
            };

            if (century == -1)
            {
                error = "الرقم القومي غير صحيح (خانة القرن يجب ان تكون 2 او 3)";
                return false;
            }

            var yy = int.Parse(nationalId.Substring(1, 2));
            var mm = int.Parse(nationalId.Substring(3, 2));
            var dd = int.Parse(nationalId.Substring(5, 2));
            var year = century + yy;

            if (mm < 1 || mm > 12)
            {
                error = "الرقم القومي غير صحيح (الشهر غير صالح)";
                return false;
            }

            if (dd < 1 || dd > DateTime.DaysInMonth(year, mm))
            {
                error = "الرقم القومي غير صحيح (اليوم غير صالح لهذا الشهر)";
                return false;
            }

            var decoded = new DateTime(year, mm, dd);
            if (decoded > DateTime.Today)
            {
                error = "الرقم القومي غير صحيح (تاريخ الميلاد المستخرج فى المستقبل)";
                return false;
            }

            birthDate = decoded;
            return true;
        }

        // كود المحافظة (الخانتين 8 و9) - بيفترض ان الرقم اتأكد قبل كده انه 14 رقم بالكامل
        public static bool TryDecodeGovernorate(string nationalId, out EgyptianGovernorate governorate, out string? error)
        {
            governorate = default;
            error = null;

            var code = int.Parse(nationalId.Substring(7, 2));
            if (!Enum.IsDefined(typeof(EgyptianGovernorate), code))
            {
                error = "الرقم القومي غير صحيح (كود المحافظة غير معروف)";
                return false;
            }

            governorate = (EgyptianGovernorate)code;
            return true;
        }
    }
}
