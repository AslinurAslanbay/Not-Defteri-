using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Hosting;
using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    /// <summary>
    /// Notları Firebase Firestore üzerinde saklayan repository.
    /// Ekle, sil, güncelle ve listele işlemleri Firestore'da yapılır.
    /// </summary>
    public class FirebaseNoteRepository : INoteRepository
    {
        private const string CollectionName = "notes";
        private const string ProjectId = "not-defteri-84e68";
        private readonly FirestoreDb _db;

        public FirebaseNoteRepository(IHostEnvironment env)
        {
            var keyPath = Path.Combine(env.ContentRootPath, "firebase-key.json");
            if (!File.Exists(keyPath))
                throw new InvalidOperationException($"Firebase anahtar dosyası bulunamadı: {keyPath}");

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
            _db = FirestoreDb.Create(ProjectId);
        }

        private CollectionReference NotesCollection => _db.Collection(CollectionName);

        private static Note SnapshotToNote(DocumentSnapshot snapshot)
        {
            var id = int.Parse(snapshot.Id);
            var title = snapshot.GetValue<string>("Title") ?? string.Empty;
            var description = snapshot.GetValue<string>("Description");
            var createdValue = snapshot.GetValue<Timestamp>("CreatedDate");
            var createdDate = createdValue.ToDateTime();

            return new Note
            {
                Id = id,
                Title = title,
                Description = description,
                CreatedDate = createdDate
            };
        }

        private static Dictionary<string, object> NoteToFirestoreData(Note note)
        {
            return new Dictionary<string, object>
            {
                ["Title"] = note.Title ?? string.Empty,
                ["Description"] = note.Description ?? string.Empty,
                ["CreatedDate"] = Timestamp.FromDateTime(DateTime.SpecifyKind(note.CreatedDate, DateTimeKind.Utc))
            };
        }

        public List<Note> GetAll()
        {
            var snapshot = NotesCollection.GetSnapshotAsync().GetAwaiter().GetResult();
            var list = snapshot.Documents
                .Select(doc => SnapshotToNote(doc))
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
            return list;
        }

        public Note? GetById(int id)
        {
            var docRef = NotesCollection.Document(id.ToString());
            var snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
            if (!snapshot.Exists)
                return null;
            return SnapshotToNote(snapshot);
        }

        public void Add(Note note)
        {
            var all = GetAll();
            var newId = all.Count > 0 ? all.Max(n => n.Id) + 1 : 1;
            note.Id = newId;
            note.CreatedDate = DateTime.Now;

            var data = NoteToFirestoreData(note);
            NotesCollection.Document(note.Id.ToString()).SetAsync(data).GetAwaiter().GetResult();
        }

        public void Update(Note note)
        {
            var docRef = NotesCollection.Document(note.Id.ToString());
            var snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
            if (!snapshot.Exists)
                return;

            var existing = SnapshotToNote(snapshot);
            existing.Title = note.Title;
            existing.Description = note.Description;
            var data = NoteToFirestoreData(existing);
            docRef.SetAsync(data).GetAwaiter().GetResult();
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                NotesCollection.Document(id.ToString()).DeleteAsync().GetAwaiter().GetResult();
            }
        }
    }
}
