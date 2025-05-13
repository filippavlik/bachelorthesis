namespace AdminPart.DTOs
{
    public class RefereeLevelDto : IRefereeDto
    {
        public RefereeLevelDto(string name, string surname, int league, int age)
        {
            Name = name;
            Surname = surname;
            League = league;
            Age = age;
            Ofs = true;
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public int League { get; set; }
        public int Age { get; set; }
        public bool Ofs { get; set; }

        
    }
}
