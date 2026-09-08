using HRMS.Domain.Entities;

namespace HRMS.Web.ViewModels
{
    public class DepartmentsIndexViewModel
    {
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
        public DepartmentFormViewModel Form { get; set; } = new();
    }
}
