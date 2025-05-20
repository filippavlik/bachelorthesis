namespace AdminPart.DTOs
{
    public class FilledFieldDto
    {
        public string FieldName;
        public string? FieldAddress;
        public float? FieldLatitude;
        public float? FieldLongtitude;

        public FilledFieldDto(string fieldName, string? fieldAddress, float? fieldLatitude, float? fieldLongtitude)
        {
            this.FieldName = fieldName;
            this.FieldAddress = fieldAddress;
            this.FieldLatitude = fieldLatitude;
            this.FieldLongtitude = fieldLongtitude;
        }
    }
}
