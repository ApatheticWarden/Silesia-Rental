using System;

namespace SilesiaRental.Models
{
    public class Rental
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int ToolID { get; set; }

        // ВОТ ЭТА ХРЕНЬ, КОТОРОЙ НЕ ХВАТАЛО:
        public Tool? Tool { get; set; }
    }
}