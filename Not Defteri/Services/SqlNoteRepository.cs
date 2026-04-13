using NotDefteriMvc.Data;
using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    public class SqlNoteRepository : INoteRepository
    {
        private readonly AppDbContext _context;

        public SqlNoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Note> GetAll(int userId)
        {
            return _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        public Note? GetById(int id, int userId)
        {
            return _context.Notes.FirstOrDefault(n => n.Id == id && n.UserId == userId);
        }

        public void Add(Note note)
        {
            note.CreatedDate = DateTime.Now;
            _context.Notes.Add(note);
            _context.SaveChanges();
        }

        public void Update(Note note)
        {
            var existing = _context.Notes.FirstOrDefault(n => n.Id == note.Id && n.UserId == note.UserId);
            if (existing == null)
            {
                return;
            }

            existing.Title = note.Title;
            existing.Description = note.Description;
            existing.IsFavorite = note.IsFavorite;

            _context.SaveChanges();
        }

        public void DeleteMany(IEnumerable<int> ids, int userId)
        {
            var notesToDelete = _context.Notes
                .Where(n => ids.Contains(n.Id) && n.UserId == userId)
                .ToList();

            if (notesToDelete.Count == 0)
            {
                return;
            }

            _context.Notes.RemoveRange(notesToDelete);
            _context.SaveChanges();
        }
    }
}
