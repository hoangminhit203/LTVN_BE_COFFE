namespace LVTN_BE_COFFE.Domain.Common
{
    public static class Numbers
    {
        public const int One = 1;
        public struct Calendar
        {
            public const int Months = 12;
            public const int Weeks = 54;
            public const int Days = 365;
        }
        public struct FindResponse
        {
            public const int NotFound = 0;
            public const int Success = 1;
            public const int Error = -1;
        }
        public struct Pagination
        {
            public const int DefaultPageSize = 10;
            public const int DefaultPageNumber = 1;
            public static readonly int[] DefaultRecordLimit = [10, 25, 50, 100, 150, 200];
        }
    }
}
