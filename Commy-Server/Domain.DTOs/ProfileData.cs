namespace Domain.DTOs
{
    public class ProfileData
    {
        public long Id { get; set; }
        public long DefaultCommunity { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public long Birthday { get; set; }
        public long LastOpenedNotifications { get; set; }
        public string ImageURI { get; set; } = string.Empty;
    }
}