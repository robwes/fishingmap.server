namespace FishingMap.Domain.DTO.Users
{
    public class UserCredentials
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
