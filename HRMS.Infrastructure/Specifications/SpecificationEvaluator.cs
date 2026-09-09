using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Specifications
{
    // بيحوّل أي ISpecification لـ IQueryable حقيقي فوق EF Core
    public static class SpecificationEvaluator<T> where T : BaseEntity
    {
        // evaluateCriteriaOnly=true بتترك بس الـ Where، مفيدة للـ Count (من غير Include/Order/Paging)
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec, bool evaluateCriteriaOnly = false)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            if (evaluateCriteriaOnly)
                return query;

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);

            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            return query;
        }
    }
}
