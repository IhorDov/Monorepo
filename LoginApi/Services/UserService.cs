using LoginApi.Context;
using LoginApi.Models;
using LoginApi.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginApi.Services
{
    public class UserService
    {
        private readonly IAsyncRepository<UserDbContext> _repository;
        private readonly IConfiguration _configuration;

        public UserService(IAsyncRepository<UserDbContext> repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task RegisterUser(UserDto userDto)
        {
            var existingUser = await _repository.GetItem<User>(u => u.Where(x => x.UserName == userDto.UserName));

            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = userDto.UserName,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
                };

                await _repository.AddItem(user);
            }
            else
            {
                throw new Exception("User already exists.");
            }
        }

        public async Task<string?> LoginUser(UserDto userDto)
        {
            // Check if user exists in the repository
            var existingUser = await _repository.GetItem<User>(u => u.Where(x => x.UserName == userDto.UserName));

            // Validate the password using BCrypt
            if (existingUser != null && BCrypt.Net.BCrypt.Verify(userDto.Password, existingUser.Password))
            {
                // Log the successful login attempt
                Console.WriteLine("User login successful");

                // Return the JWT token
                return CreateToken(existingUser);
            }

            // Return null if authentication fails
            return null;
        }

        public async Task<List<User>> GetAllUsers()
        {
            // Using the generic repository to fetch all users
            return await _repository.GetAllItems<User>();
        }

        public async Task DeleteUser(int id)
        {

            await _repository.RemoveItem<User>(x => x.Id == id);
        }

        public async Task DeleteAll()
        {
            await _repository.RemoveItems<User>(x => x.Where(z => z.UserName != string.Empty));
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName) //ClaimTypes.NameIdentifier - using DB
                //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                //new Claim(ClaimTypes.Role, "Admin"),
            };

            // Use the token from the configuration which was set from .env in Program.cs
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: "http://loginapi",
                audience: "http://loginapi",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            //Last step is to write the token
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            Console.WriteLine($"Generated JWT: {jwt}");

            return jwt;
        }
    }
}
