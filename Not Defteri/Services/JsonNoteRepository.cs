using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    public class JsonNoteRepository : INoteRepository
    {
        private readonly string _jsonFilePath;

        public JsonNoteRepository(IHostEnvironment env)
        {
            _jsonFilePath = Path.Combine(env.ContentRootPath, "notlar.json");
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_jsonFilePath))
            {
                var emptyList = new List<Note>();
                var json = JsonSerializer.Serialize(emptyList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_jsonFilePath, json);
            }
        }

        private List<Note> ReadAllFromFile()
        {
            var json = File.ReadAllText(_jsonFilePath);
            return JsonSerializer.Deserialize<List<Note>>(json) ?? [];
        }

        private void WriteAllToFile(List<Note> notes)
        {
            var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_jsonFilePath, json);
        }

        public List<Note> GetAll(int userId)
        {
            return ReadAllFromFile()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        public Note? GetById(int id, int userId)
        {
            return ReadAllFromFile().FirstOrDefault(n => n.Id == id && n.UserId == userId);
        }

        public void Add(Note note)
        {
            var notes = ReadAllFromFile();
            var newId = notes.Any() ? notes.Max(n => n.Id) + 1 : 1;
            note.Id = newId;
            note.CreatedDate = DateTime.Now;

            notes.Add(note);
            WriteAllToFile(notes);
        }

        public void Update(Note note)
        {
            var notes = ReadAllFromFile();
            var existing = notes.FirstOrDefault(n => n.Id == note.Id && n.UserId == note.UserId);
            if (existing == null)
            {
                return;
            }

            existing.Title = note.Title;
            existing.Description = note.Description;
            existing.IsFavorite = note.IsFavorite;

            WriteAllToFile(notes);
        }

        public void DeleteMany(IEnumerable<int> ids, int userId)
        {
            var idSet = ids.ToHashSet();
            var notes = ReadAllFromFile();

            notes = notes.Where(n => !(idSet.Contains(n.Id) && n.UserId == userId)).ToList();

            WriteAllToFile(notes);
        }
    }
}
