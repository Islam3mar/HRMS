using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Enums
{
    public static class EgyptianGovernorateExtensions
    {
        public static string ToArabicName(this EgyptianGovernorate governorate) => governorate switch
        {
            EgyptianGovernorate.Cairo => "القاهرة",
            EgyptianGovernorate.Alexandria => "الاسكندرية",
            EgyptianGovernorate.PortSaid => "بورسعيد",
            EgyptianGovernorate.Suez => "السويس",
            EgyptianGovernorate.Damietta => "دمياط",
            EgyptianGovernorate.Dakahlia => "الدقهلية",
            EgyptianGovernorate.Sharqia => "الشرقية",
            EgyptianGovernorate.Qalyubia => "القليوبية",
            EgyptianGovernorate.KafrElSheikh => "كفر الشيخ",
            EgyptianGovernorate.Gharbia => "الغربية",
            EgyptianGovernorate.Monufia => "المنوفية",
            EgyptianGovernorate.Beheira => "البحيرة",
            EgyptianGovernorate.Ismailia => "الاسماعيلية",
            EgyptianGovernorate.Giza => "الجيزة",
            EgyptianGovernorate.BeniSuef => "بنى سويف",
            EgyptianGovernorate.Fayoum => "الفيوم",
            EgyptianGovernorate.Minya => "المنيا",
            EgyptianGovernorate.Assiut => "اسيوط",
            EgyptianGovernorate.Sohag => "سوهاج",
            EgyptianGovernorate.Qena => "قنا",
            EgyptianGovernorate.Aswan => "اسوان",
            EgyptianGovernorate.Luxor => "الاقصر",
            EgyptianGovernorate.RedSea => "البحر الاحمر",
            EgyptianGovernorate.NewValley => "الوادى الجديد",
            EgyptianGovernorate.Matrouh => "مطروح",
            EgyptianGovernorate.NorthSinai => "شمال سيناء",
            EgyptianGovernorate.SouthSinai => "جنوب سيناء",
            EgyptianGovernorate.BornAbroad => "مواليد خارج مصر",
            _ => governorate.ToString()
        };
    }
}
