namespace Shop_API.Models.AuthModels
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public byte[] TokenHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt {  get; set; }

        public DateTime? RevokedAt { get; set; }

        public int? ReplacedByTokenId { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
