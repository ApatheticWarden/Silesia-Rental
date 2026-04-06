using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SilesiaRental.Models {
    public class User {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }

        public string Role { get; set; } = "User";

        public User() { }
    }
}
