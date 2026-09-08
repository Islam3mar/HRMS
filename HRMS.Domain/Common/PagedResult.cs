using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Common
{
    // نتيجة عامة لأي Pagination في المشروع (اتعملت هنا عشان اي Repository يقدر يستخدمها)
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
