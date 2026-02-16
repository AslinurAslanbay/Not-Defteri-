namespace NotDefteriMvc.Models
{
    public class Note
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        // Favori notlar için bayrak
        public bool IsFavorite { get; set; } = false;
    }
}

