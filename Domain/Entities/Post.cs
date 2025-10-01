namespace Tbilink_Back.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Content { get; set; }   
        public string? ImageUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

}
