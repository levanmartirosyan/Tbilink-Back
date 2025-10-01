namespace Tbilink_Back.Models
{
    public class UserPhoto
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
