using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Pagination
{
    public class PaginationResult<T>
    {
        public IEnumerable<T> Items {get; set;} = new List<T>();
        public int TotalCount {get; set;}
        public int PageNumber {get; set;}
        public int PageSize {get; set;}
        public int TotalPages => (int) Math.Ceiling((double)TotalCount / PageSize);

    }
}

// Items 
// totalCount 
// pageNumber
// pageSize 
// totalPage