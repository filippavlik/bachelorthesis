using AdminPart.Models;

namespace AdminPart.DTOs
{
    public class FilledMatchDto
    {
       

        public FilledMatchDto(string numberMatch, string idOfReferee, string nameOfReferee, string surnameOfReferee
                                                , string idOfAr1, string nameOfAr1, string surnameOfAr1
                                                , string idOfAr2, string nameOfAr2, string surnameOfAr2
                            )
        {
            this.NumberMatch = numberMatch;
            this.IdOfReferee = idOfReferee;
            this.NameOfReferee = nameOfReferee;
            this.SurnameOfReferee = surnameOfReferee;
            this.IdOfAr1 = idOfAr1;
            this.NameOfAr1 = nameOfAr1;
            this.SurnameOfAr1 = surnameOfAr1;
            this.IdOfAr2 = idOfAr2;
            this.NameOfAr2 = nameOfAr2;
            this.SurnameOfAr2 = surnameOfAr2;
        }

        public string NumberMatch;
        public string? IdOfReferee;
        public string? NameOfReferee;
        public string? SurnameOfReferee;

        public string? IdOfAr1;
        public string? NameOfAr1;
        public string? SurnameOfAr1;

        public string? IdOfAr2;
        public string? NameOfAr2;
        public string? SurnameOfAr2;
    }
}