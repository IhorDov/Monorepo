using LoginApi.Models;

namespace LoginApi.Services
{
    public interface IUserService
    {
        Task RegisterUser(UserDto userDto);
        Task<string?> LoginUser(UserDto userDto);
        Task<List<User>> GetAllUsers();
    }
}
