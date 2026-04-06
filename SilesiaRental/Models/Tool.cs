using System.ComponentModel.DataAnnotations;

namespace SilesiaRental.Models {
    public class Tool {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
