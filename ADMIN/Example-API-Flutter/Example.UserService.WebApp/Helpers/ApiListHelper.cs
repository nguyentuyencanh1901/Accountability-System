using Example.Common.Models;

namespace Example.UserService.WebApp.Helpers
{
    public static class ApiListHelper
    {
        public static (List<T> Items, long TotalItems, string? ErrorMessage) ParseList<T>(ResponseData<List<T>>? result)
            where T : class
        {
            if (result == null || !result.Success)
                return (new List<T>(), 0, result?.Message ?? "Không thể tải dữ liệu từ API.");

            var items = result.Data ?? new List<T>();
            var total = result.MetaData?.TotalItems ?? items.Count;
            return (items, total, null);
        }
    }
}
