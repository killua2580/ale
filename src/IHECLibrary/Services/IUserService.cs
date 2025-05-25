using System.Collections.Generic;
using System.Threading.Tasks;

namespace IHECLibrary.Services
{
    public interface IUserService
    {
        Task<UserModel?> GetCurrentUserAsync();
        Task<UserProfileModel?> GetCurrentUserProfileAsync();
        Task<UserModel?> GetUserByIdAsync(string userId);
        Task<List<UserModel>> SearchUsersAsync(string searchQuery);
        Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model);
        Task<UserStatisticsModel> GetUserStatisticsAsync(string userId);
    }
}
