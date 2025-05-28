namespace AdminPartDevelop.DTOs
{
    public class FilledRefereeDto : IRefereeDto
    {
        public FilledRefereeDto(string name, string surname, string? email, string? facrId , string? phoneNumber)
        {
            Name = name;
            Surname = surname;
            Email = email;
            FacrId = facrId;
            PhoneNumber = phoneNumber;
        }

       
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Email { get; set; }
        public string? FacrId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
