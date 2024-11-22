namespace ConsoleApp.Models
{
    public class EchoRequest
    {
        public string? EchoString { get; set; }
    }

    public class EchoResponse
    {
        public string? EchoString { get; set; }
        public DateTime ServerTime { get; set; }
    }
}
