using Azure.Core;
using Microsoft.EntityFrameworkCore;
using SilesiaRental.Data;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;

namespace SilesiaRental.Services
{
    public class RentalService : IRentalService
    {
        private readonly SilesiaRentalAPIContext _context;

        public RentalService(SilesiaRentalAPIContext context)
        {
            _context = context;
        }

        // Just rent tool
        public async Task<Envelope<Rental>> RentToolAsync(RentRequestDTO req, string customerName)
        {
            try {
                var tool = await _context.Tools.FindAsync(req.ToolId);
                if (tool == null) return Envelope<Rental>.Error("No such instrument");

                if (!tool.IsAvailable) return Envelope<Rental>.Error("This instrument is not avaliable");

                // Creating rent
                var rentalEntity = new Rental {
                    ToolID = req.ToolId,
                    CustomerName = customerName,
                    RentalDate = DateTime.Now
                };

                _context.Rentals.Add(rentalEntity);

                tool.IsAvailable = false; // Flag !isAvailable

                _context.Entry(tool).Property(t => t.Version).OriginalValue = req.Version;

                await _context.SaveChangesAsync();
                return Envelope<Rental>.Ok(rentalEntity, "Tool was rented successfuly!");
            } catch (DbUpdateConcurrencyException){
                return Envelope<Rental>.Error("Someone just grabbed this tool milliseconds before you! Please refresh.");
            }
        }

        public async Task<Envelope<RentalCheckDTO>> ReturnToolAsync(int rentalId) {
            // getting particular rental with id
            var rental = await _context.Rentals
                .Include(r => r.Tool)
                .FirstOrDefaultAsync(r => r.ID == rentalId);

            if (rental == null) return Envelope<RentalCheckDTO>.Error($"No rental with this id {rentalId}");
            if (rental.ReturnDate != null) return Envelope<RentalCheckDTO>.Error("This tool is already returned!");

            // Days spent:
            var timeSpan = DateTime.Now - rental.RentalDate;
            var days = timeSpan.Days == 0 ? 1 : timeSpan.Days;

            decimal price = rental.Tool?.PricePerDay ?? 0;
            decimal total = days * price;

            rental.ReturnDate = DateTime.Now;
            if (rental.Tool != null) rental.Tool.IsAvailable = true;

            await _context.SaveChangesAsync();

            // Bill
            return Envelope<RentalCheckDTO>.Ok(new RentalCheckDTO {
                Days = days,
                TotalPrice = total,
                ToolName = rental.Tool?.Name ?? "Unknown"
            });
        }

        // Get all rentals
        public async Task<Envelope<List<Rental>>> GetUserRentalsAsync(string username) {
            return Envelope<List<Rental>>.Ok(await _context.Rentals
                .Include(r => r.Tool)
                .Where(r => r.CustomerName == username)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync());
        }
    }
}