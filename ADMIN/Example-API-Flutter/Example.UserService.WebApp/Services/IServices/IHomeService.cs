using Example.UserService.WebApp.Models.Home;

namespace Example.UserService.WebApp.Services.IServices
{
    public interface IHomeService
    {
        Task<DashboardViewModel> BuildDashboardAsync(long userId, string? fullName, string? username);
    }
}
