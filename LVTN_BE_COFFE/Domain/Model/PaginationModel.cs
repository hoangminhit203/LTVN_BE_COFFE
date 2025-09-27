namespace LVTN_BE_COFFE.Domain.Model
{
    public class PaginationModel<T> where T : class
    {
        public long TotalRecords { get; set; }
        public required IEnumerable<T> Records { get; set; }
    }
    public class PagedList<T> where T : class
    {
        public long Total { get; set; }
        public required IEnumerable<T> Items { get; set; }
    }
}
