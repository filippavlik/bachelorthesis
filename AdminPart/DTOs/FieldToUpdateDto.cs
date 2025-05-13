namespace AdminPart.DTOs
{
    public class FieldToUpdateDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string? FieldAddress { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
