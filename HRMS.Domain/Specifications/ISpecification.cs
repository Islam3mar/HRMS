using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Specifications
{
    // العقد العام لأي Specification: بيوصف "عايزين إيه" (شرط + Includes + ترتيب + صفحات)
    // من غير ما الـ Repository يعرف تفاصيل الاستعلام نفسه
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>>? Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }   // لحالات الـ ThenInclude زي "Employee.Department"

        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }

        int Skip { get; }
        int Take { get; }
        bool IsPagingEnabled { get; }
    }
}
