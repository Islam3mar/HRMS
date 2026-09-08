using HRMS.Domain.Entities;

namespace HRMS.Web.ViewModels
{
    public class OfficialHolidaysIndexViewModel
    {
        public OfficialHolidayFormViewModel Form { get; set; } = new();
        public IEnumerable<OfficialHoliday> Holidays { get; set; } = new List<OfficialHoliday>();
    }
}
