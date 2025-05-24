using backend.Dtos;
using backend.Repositories;

namespace backend.Services
{
    public class AdminService
    {
        private readonly AdminRepository _adminRepository;

        public AdminService(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<object> GetAllUsers(int adminId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.GetAllUsersWithRoles();
        }

        public async Task<object> UpdateUser(int adminId, int userId, UserUpdateDto userDto)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.UpdateUserWithRole(userId, userDto);
        }

        public async Task<List<object>> GetAllRoles()
        {
            return await _adminRepository.GetAllRoles();
        }

        public async Task<object> DeleteUser(int adminId, int userId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.DeleteUserWithDependencies(userId);
        }

        public async Task<object> CreateUser(int adminId, AdminUserCreationDto userDto)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.CreateUserWithRole(userDto);
        }

        public async Task<List<AdminTutorialDto>> GetAllTutorials(int adminId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.GetAllTutorialsWithDetails();
        }

        public async Task<object> UpdateTutorialContents(int adminId, int tutorialId, TutorialContentUpdateDto contentDto)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.UpdateTutorialContents(tutorialId, contentDto);
        }

        public async Task<object> DeleteTutorial(int adminId, int tutorialId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.DeleteTutorialWithContents(tutorialId);
        }

        public async Task<List<object>> GetAllCategories()
        {
            return await _adminRepository.GetAllCategories();
        }

        public async Task<List<AdminTutorialDto>> GetPendingTutorials(int adminId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.GetPendingTutorialsWithDetails();
        }

        public async Task<object> ApproveTutorial(int adminId, int tutorialId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.ApproveTutorial(tutorialId);
        }

        public async Task<List<UserStatisticsDto>> GetUserStatistics(int adminId)
        {
            await _adminRepository.ValidateAdmin(adminId);
            return await _adminRepository.GetUserStatistics();
        }
    }
}