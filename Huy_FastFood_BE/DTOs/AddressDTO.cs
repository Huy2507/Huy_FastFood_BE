namespace Huy_FastFood_BE.DTOs
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string Street { get; set; } = null!;
        public string Ward { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
