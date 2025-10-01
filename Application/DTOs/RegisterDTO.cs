namespace Tbilink_Back.DTOs
{
    public class RegisterDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RepPassword { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Description { get; set; }
    }
}
