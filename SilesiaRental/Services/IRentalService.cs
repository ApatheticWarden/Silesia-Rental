using SilesiaRental.Models;
using SilesiaRental.DTOs;
using SilesiaRental.Models.Wrapper;

namespace SilesiaRental.Services {
    public interface IRentalService {
        Task<Envelope<Rental>> RentToolAsync(RentRequestDTO req, string customerName);

        Task<Envelope<RentalCheckDTO>> ReturnToolAsync(int rentalId);

        Task<Envelope<List<Rental>>> GetUserRentalsAsync(string customerName);
    }
}