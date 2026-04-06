using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilesiaRental.Data;
using SilesiaRental.DTOs;
using SilesiaRental.Models;
using SilesiaRental.Models.Wrapper;
using SilesiaRental.Services;

namespace SilesiaRental.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class ToolsController : ControllerBase {        
        // Connect our service
        private readonly IToolService _toolService;

        //Options from Program.cs
        public ToolsController(IToolService toolService) {
            _toolService = toolService;
        }

        [HttpGet]
        public async Task<ActionResult<Envelope<List<Tool>>>> GetTools(string? name, int? maxPrice) {
            var envelope = await _toolService.GetToolsAsync(name, maxPrice);

            var sortedList = envelope.Payload!.Where(t => t.IsAvailable == true)
                .OrderByDescending(t => t.ID).ToList();
            
            return Ok(Envelope<List<Tool>>.Ok(sortedList));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToolResponseDTO>> GetToolByID(int id) {
            var env = await _toolService.GetToolByIdAsync(id);
            if (env.Payload == null) return NotFound();
            var response = new ToolResponseDTO {
                Id = env.Payload!.ID,
                Name = env.Payload!.Name,
                PricePerDay = env.Payload!.PricePerDay,
                Version = env.Payload!.Version
            };
            return Ok(response);
        }

        // Logic: 
        // Request coming from user
        // 1) Take evrthn from request, make it Tool
        // 2) Provide as arg in CreateToolAsync and create an response
        // with data we want to show
        [HttpPost]
        public async Task<IActionResult> CreateTool(CreateToolDTO request) {
            // From request
            var tool = new Tool {
                Name = request.Name,
                Description = request.Description,
                PricePerDay = request.PricePerDay
            };
            // Why so? 
            // We can add to tool smthn, but this will crash whole srv

            // Creating actual Tool
            var createdTool = await _toolService.CreateToolAsync(tool);

            // Creating an response to user
            var response = new ToolResponseDTO {
                Id = createdTool.Payload!.ID,
                Name = createdTool.Payload!.Name,
                PricePerDay = createdTool.Payload!.PricePerDay
            };

            return CreatedAtAction(nameof(GetToolByID), new { id = response.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(int id) {
            var toolToDel = await _toolService.DeleteToolAsync(id);
            if (!toolToDel.IsSuccess) return BadRequest(toolToDel.Message);

            return Ok("Tool was deleted");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTool(int id, Tool updatedTool) {
            var tool = await _toolService.UpdateToolAsync(id, updatedTool);
            if (!tool.IsSuccess) return BadRequest(tool.Message);

            return Ok(tool.Payload);
        }


    }
}
