using NotDefteriMvc.Models;

namespace NotDefteriMvc.ViewModels
{
    public class NotesIndexViewModel
    {
        public bool IsAuthenticated { get; set; }

        public bool SelectMode { get; set; }

        public string? SearchQuery { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public int? CurrentUserId { get; set; }

        public string? CurrentUserName { get; set; }

        public NoteDraft Draft { get; set; } = new();

        public List<Note> Notes { get; set; } = [];
    }
}
