namespace ConsoleApp.Models
{
    public class ErrorResponse
    {
        public Error? Error { get; set; }

    }
    public class Error
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
