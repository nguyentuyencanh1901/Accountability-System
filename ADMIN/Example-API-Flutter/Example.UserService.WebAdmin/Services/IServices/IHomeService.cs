using Example.UserService.WebAdmin.Models;

namespace Example.UserService.WebAdmin.Services.IServices
{
    public interface IHomeService
    {
        Task<DashboardViewModel> BuildDashboardAsync(string? fullName, string? username);
    }
}
