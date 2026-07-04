using Example.UserService.WebApp.Models.Profile;

namespace Example.UserService.WebApp.Services.IServices
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileAsync(long userId);
        Task<EditProfileViewModel> BuildEditFormAsync(long userId);
        Task<(bool Success, string? ErrorMessage, string? FullName, string? Email)> UpdateProfileAsync(long userId, EditProfileViewModel model);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(long userId, ChangePasswordViewModel model);
    }
}
