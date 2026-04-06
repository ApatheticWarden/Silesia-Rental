using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SilesiaRental.Data;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using SilesiaRental.Services;
using System.Text.Json;

namespace SilesiaRental.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RentalsController : ControllerBase {
        // Urgent
        // Теперь мы зависим не от Базы Данных, а от Сервиса
        private readonly IRentalService _rentalService;

        // Конструктор: ASP.NET сам "вставит" сюда нужный сервис
        public RentalsController(IRentalService rentalService) {
            _rentalService = rentalService;
        }

        // Other stuff

        /// <summary>
        /// Allows user to rent tool
        /// </summary>
        /// <param name="rentDTO"></param>
        /// <returns>State</returns>
        [HttpPost("rent")]
        public async Task<IActionResult> RentTool([FromBody] RentRequestDTO request)
        {
            string username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username)) return Unauthorized();

            // Передаем в сервис ID инструмента и Имя из токена
            var result = await _rentalService.RentToolAsync(request, username);

            if (!result.IsSuccess) return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Allows user to return tool and counts price of rent
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Check with total price and days count</returns>
        [HttpPost("return/{id}")]
        public async Task<IActionResult> ReturnTool(int id) {
            var check = await _rentalService.ReturnToolAsync(id);

            if (check == null) {
                return BadRequest("Not found or the tool is not available");
            }

            return Ok(check);
        }

        [HttpGet("active-rentals")]
        public async Task<IActionResult> GetActiveRentals()
        {
            string username = User.Identity?.Name!;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            // Getting an envelope from Service
            var envelope = await _rentalService.GetUserRentalsAsync(username);
            if (!envelope.IsSuccess) return BadRequest(envelope);

            var sortedList = envelope.Payload!.Where(r => r.ReturnDate == null)
                .OrderByDescending(r => r.RentalDate).ToList();
            
            return Ok(Envelope<List<Rental>>.Ok(sortedList));
        }

        [HttpGet("closed-rentals")]
        public async Task<IActionResult> GetClosedRentals() {
            string username = User.Identity?.Name!;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            // Getting an envelope from Service
            var envelope = await _rentalService.GetUserRentalsAsync(username);
            if (!envelope.IsSuccess) return BadRequest(envelope);

            var sortedList = envelope.Payload!.Where(r => r.ReturnDate != null)
                .OrderByDescending(r => r.RentalDate).ToList();

            return Ok(Envelope<List<Rental>>.Ok(sortedList));
        }
    }
}
