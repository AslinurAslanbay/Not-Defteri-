using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    public interface INoteRepository
    {
        List<Note> GetAll(int userId);
        Note? GetById(int id, int userId);
        void Add(Note note);
        void Update(Note note);
        void DeleteMany(IEnumerable<int> ids, int userId);
    }
}
