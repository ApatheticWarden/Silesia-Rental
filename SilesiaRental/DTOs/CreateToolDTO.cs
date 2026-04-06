using SilesiaRental.Models;

namespace SilesiaRental.DTOs {
    public class CreateToolDTO {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerDay { get; set; }
    }
}

// Some explanation to DTO (Data Transfer Object)
// We introduce intermediaries.
// Entity(Tool): Lives only in the database and within DbContext.
// DTO (ToolDto): An object for communication with the external world (JSON).
// It's like in a restaurant:
//Entity — the raw ingredients in the refrigerator (raw meat, sack of potatoes).
// DTO — the menu item ("Steak with potatoes").
// The customer orders from the Menu (DTO), and the chef takes the Ingredients (Entity).


