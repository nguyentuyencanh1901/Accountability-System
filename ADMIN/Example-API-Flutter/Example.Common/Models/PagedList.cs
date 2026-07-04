using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Models
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, long totalItems, int pageIndex, int pageSize)
        {
            _metaData = new PagedMetaData
            {
                TotalItems = totalItems,
                PageSize = pageSize,
                CurrentPage = pageIndex,
                // TotalPages = (int) Math.Ceiling (totalItems / (double) pageSize)
            };
            AddRange(items);
        }

        private PagedMetaData _metaData { get; }
        public PagedMetaData GetMetaData() => _metaData;
    }
}
