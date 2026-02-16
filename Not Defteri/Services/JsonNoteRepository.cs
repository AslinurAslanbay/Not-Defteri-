using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    // Bu sınıf, INoteRepository arayüzünü JSON dosyası kullanarak uygular.
    public class JsonNoteRepository : INoteRepository
    {
        private readonly string _jsonFilePath;

        // IHostEnvironment ile uygulamanın ContentRootPath (proje kökü) yolunu alıyoruz.
        public JsonNoteRepository(IHostEnvironment env)
        {
            // notlar.json proje köküne (ContentRoot) konumlandırılıyor
            _jsonFilePath = Path.Combine(env.ContentRootPath, "notlar.json");
            EnsureFileExists();
        }

        // Uygulama ilk açıldığında notlar.json yoksa oluşturan metot
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

        // JSON dosyasını okuyup Note listesini döner
        private List<Note> ReadAllFromFile()
        {
            var json = File.ReadAllText(_jsonFilePath);
            var notes = JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
            return notes;
        }

        // Note listesini JSON dosyasına yazar
        private void WriteAllToFile(List<Note> notes)
        {
            var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_jsonFilePath, json);
        }

        public List<Note> GetAll()
        {
            return ReadAllFromFile()
                .OrderByDescending(n => n.CreatedDate) // Son eklenen üstte görünsün
                .ToList();
        }

        public Note? GetById(int id)
        {
            return ReadAllFromFile().FirstOrDefault(n => n.Id == id);
        }

        public void Add(Note note)
        {
            var notes = ReadAllFromFile();

            // Yeni Id üret (maks Id + 1 mantığı)
            var newId = notes.Any() ? notes.Max(n => n.Id) + 1 : 1;
            note.Id = newId;
            note.CreatedDate = DateTime.Now;

            notes.Add(note);
            WriteAllToFile(notes);
        }

        public void Update(Note note)
        {
            var notes = ReadAllFromFile();
            var existing = notes.FirstOrDefault(n => n.Id == note.Id);

            if (existing == null)
                return;

            // Sadece Title, Description ve IsFavorite güncelleniyor, CreatedDate aynen kalabilir
            existing.Title = note.Title;
            existing.Description = note.Description;
            existing.IsFavorite = note.IsFavorite;

            WriteAllToFile(notes);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            var idSet = ids.ToHashSet();
            var notes = ReadAllFromFile();

            notes = notes.Where(n => !idSet.Contains(n.Id)).ToList();

            WriteAllToFile(notes);
        }
    }
}

