namespace LoginPart.Models
{
    public class AllowedEmailAddress
    {
        public int Id { get; set; } // Primary Key
        public string Email { get; set; } // Email Address
        public int Role { get; set; } // Role (e.g., 0 for Admin, 1 for Referee)
    }
}
