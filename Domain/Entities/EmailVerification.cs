namespace Tbilink_Back.Models
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string CodeHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }


}
