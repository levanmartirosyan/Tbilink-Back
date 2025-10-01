namespace Tbilink_Back.DTOs
{
    public class RequestVerificationDTO
    {
        public string Email { get; set; }
    }

    public class VerifyEmailDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
