namespace SilesiaRental.DTOs {
    public class ToolResponseDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal PricePerDay { get; set; }

        public byte[] Version { get; set; }
    }
}
