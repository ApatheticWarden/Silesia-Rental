namespace SilesiaRental.Models.Wrapper
{
    // Class that realises all work with Task<(string? Error, UserProfile Profile)>
    // And other kind of stuff
    public class Envelope<T>{
        // Data we want to transfer
        // Defined by T
        public T? Payload { get; set; }

        // Just to make logic "easier"
        public bool IsSuccess { get; set; }

        // Message we will deliver in different situation
        public string? Message { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public Envelope() { }

        // Usefull methods:

        // Action success
        public static Envelope<T> Ok(T payload, string m = "Success") {
            return new Envelope<T> {
                Payload = payload,
                IsSuccess = true,
                Message = m
            };
        }

        // Action error
        public static Envelope<T> Error(string errorMessage) {
            return new Envelope<T> {
                Payload = default,
                IsSuccess = false,
                Message = errorMessage
            };
        }
    }
}
