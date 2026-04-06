using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilesiaRental.Data;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using System.Data;

namespace SilesiaRental.Services {
    public class ToolService : IToolService {
        private readonly SilesiaRentalAPIContext _context;

        public ToolService(SilesiaRentalAPIContext context) {
            _context = context;
        }
        public async Task<Envelope<List<Tool>>> GetToolsAsync(string? name, decimal? maxPrice) {
            var query = _context.Tools.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(t => t.Name.Contains(name));

            if (maxPrice != null)
                query = query.Where(t => t.PricePerDay <= maxPrice);

            var tools = await query.ToListAsync();

            return Envelope<List<Tool>>.Ok(tools);
        }

        public async Task<Envelope<Tool>> GetToolByIdAsync(int id){
            var tool = await _context.Tools.FindAsync(id);
            if (tool == null) return Envelope<Tool>.Error($"No tool with such ID: {id}");
            return Envelope<Tool>.Ok(tool);
        }

        public async Task<Envelope<Tool>> CreateToolAsync(Tool tool) {
            tool.IsAvailable = true;

            await _context.AddAsync(tool);
            await _context.SaveChangesAsync();

            return Envelope<Tool>.Ok(tool);
        }

        // Delete tool
        public async Task<Envelope<bool>> DeleteToolAsync(int id) {
            var tool = await GetToolByIdAsync(id);
            if (tool == null) return Envelope<bool>.Error(tool.Message);

            _context.Tools.Remove(tool.Payload!);

            await _context.SaveChangesAsync();
            return Envelope<bool>.Ok(true,"Tool was successfully deleted");
        }

        // Update tool
        public async Task<Envelope<Tool>> UpdateToolAsync(int id, Tool updatedTool) {
            try {
                var env = await GetToolByIdAsync(id);

                if (!env.IsSuccess || env.Payload == null) return Envelope<Tool>.Error(env.Message);

                var toolEntity = env.Payload;

                toolEntity!.Name = updatedTool.Name;
                toolEntity!.Description = updatedTool.Description;
                toolEntity!.PricePerDay = updatedTool.PricePerDay;
                _context.Entry(toolEntity).Property(t => t.Version).OriginalValue = updatedTool.Version;

                await _context.SaveChangesAsync();

                return Envelope<Tool>.Ok(toolEntity);
            } catch (DbUpdateConcurrencyException) {
                return Envelope<Tool>.Error("The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled");
            } catch (Exception ex) {
                return Envelope<Tool>.Error(ex.Message);
            }
        }
    }
}
