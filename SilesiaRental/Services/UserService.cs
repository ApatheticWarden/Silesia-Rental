using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SilesiaRental.Data;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SilesiaRental.Services {
    public class UserService : IUserService {
        private readonly SilesiaRentalAPIContext _context;
        private readonly IConfiguration _configuration; // For JWT
        public UserService(SilesiaRentalAPIContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Envelope<bool>> RegisterUserAsync(UserDTO user) {
            if (user == null) return Envelope<bool>.Error("Provide information");

            var exists = await _context.Users.AnyAsync(u => u.Username == user.Username);
            if (exists) return Envelope<bool>.Error("User already exists");

            var newUser = new User {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password,12),
                RegistrationDate = DateTime.Now,
            };

            try {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return Envelope<bool>.Ok(true, "User created");
            } catch (Exception ex) {
                return Envelope<bool>.Error(ex.Message);
            }
        }

        private async Task<Envelope<User>> GetUserAsync(string name) { 
            var searchedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
            if (searchedUser == null) return Envelope<User>.Error("User doesn`t exist");
            return Envelope<User>.Ok(searchedUser, "User found");
        }

        public async Task<Envelope<UserProfile>> GetProfileInfo(string name) {
            var u = await GetUserAsync(name);
            if (!u.IsSuccess) return Envelope<UserProfile>.Error(u.Message!);
            
            // P stands for profile
            // User is never null. Because validating is in GetUserAsync(...)
            // Add ! after variable like here, so you guarantee that this value isn`t null
            var p = new UserProfile { Username = u.Payload!.Username };

            return Envelope<UserProfile>.Ok(p);
        }


        // Return token
        public async Task<Envelope<string>> LoginAsync(UserDTO userDTO) {
            if (userDTO == null) return Envelope<string>.Error("Provide login and password!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDTO.Username);
            if (user == null) return Envelope<string>.Error("User does not exist");

            bool isValid = BCrypt.Net.BCrypt.Verify(userDTO.Password, user.PasswordHash);
            if (!isValid) return Envelope<string>.Error("Incorrect password");

            string token = CreateToken(user);

            return Envelope<string>.Ok(token);
        }

        // Creating token method
        private string CreateToken(User user) {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            // Claims - info abt user stored in token
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),

                new Claim(ClaimTypes.Role, user.Role)
            };
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(25), // Lifetime
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
