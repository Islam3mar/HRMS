using HRMS.Domain.Enums;

namespace HRMS.Web.ViewModels
{
    public class PermissionRowViewModel
    {
        public SystemPage SystemPage { get; set; }
        public string PageName { get; set; } = string.Empty;

        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
