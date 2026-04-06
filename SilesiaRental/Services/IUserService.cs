using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;

namespace SilesiaRental.Services {
    public interface IUserService {
        Task<Envelope<bool>> RegisterUserAsync(UserDTO user);
        Task<Envelope<string>> LoginAsync(UserDTO user);
        Task<Envelope<UserProfile>> GetProfileInfo(string name);
    }
}
