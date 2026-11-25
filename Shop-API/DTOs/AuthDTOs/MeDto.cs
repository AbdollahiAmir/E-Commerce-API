namespace Shop_API.DTOs.AuthDTOs
{
    public class MeDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public List<string> Roles { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}