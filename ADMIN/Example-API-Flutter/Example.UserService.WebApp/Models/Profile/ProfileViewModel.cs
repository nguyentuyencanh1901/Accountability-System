using Example.UserService.API.Models;

namespace Example.UserService.WebApp.Models.Profile
{
    public class ProfileViewModel
    {
        public AppUserModel? User { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
