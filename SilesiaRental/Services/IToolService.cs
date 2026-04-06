using Microsoft.AspNetCore.Mvc;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;

namespace SilesiaRental.Services {
    public interface IToolService {
        Task<Envelope<List<Tool>>> GetToolsAsync(string? name, decimal? maxPrice);
        Task<Envelope<Tool>> GetToolByIdAsync(int id);
        Task<Envelope<Tool>> CreateToolAsync(Tool tool);

        Task<Envelope<bool>> DeleteToolAsync(int id);

        Task<Envelope<Tool>> UpdateToolAsync(int id, Tool updatedTool);
    }
}
