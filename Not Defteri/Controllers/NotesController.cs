using Microsoft.AspNetCore.Mvc;
using NotDefteriMvc.Models;
using NotDefteriMvc.Services;
using System.Collections.Generic;
using System.Linq;

namespace NotDefteriMvc.Controllers
{
    // CONTROLLER (C): HTTP isteklerini karşılar, Model'den veriyi alır,
    // uygun View'a (V) gönderir.
    public class NotesController : Controller
    {
        private readonly INoteRepository _noteRepository;

        // DI (Dependency Injection) ile INoteRepository örneği alıyoruz.
        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        // GET: /Notes/Index veya sadece /
        // Not listesini gösteren ana sayfa.
        // selectMode parametresi, seçim/silme modunda olup olmadığımızı belirler.
        // search parametresi, başlık ve açıklamada arama yapmak için kullanılır.
        [HttpGet]
        public IActionResult Index(bool selectMode = false, string? search = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var notes = _noteRepository.GetAll();

            // Arama filtresi uygula
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                notes = notes.Where(n =>
                    (n.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (n.Description?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }

            // Tarih filtresi uygula
            if (startDate.HasValue)
            {
                notes = notes.Where(n => n.CreatedDate.Date >= startDate.Value.Date).ToList();
            }

            if (endDate.HasValue)
            {
                notes = notes.Where(n => n.CreatedDate.Date <= endDate.Value.Date).ToList();
            }

            // ViewBag ile View'a ek veriler gönderebiliriz.
            ViewBag.SelectMode = selectMode;
            ViewBag.SearchQuery = search;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(notes);
        }

        // GET: /Notes/Create
        // Yeni not ekleme formunu gösterir.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Notes/Create
        // Form submit edildiğinde çağrılır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Note note)
        {
            if (!ModelState.IsValid)
            {
                return View(note);
            }

            _noteRepository.Add(note);

            // Kayıttan sonra liste sayfasına dön
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notes/Edit/5
        // Belirli bir notu düzenleme formunu gösterir.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: /Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Note note)
        {
            if (!ModelState.IsValid)
            {
                return View(note);
            }

            _noteRepository.Update(note);
            return RedirectToAction(nameof(Index));
        }
        // GET: /Notes/View/5
        // Belirli bir notu sadece görüntülemek için sayfayı gösterir.
        [HttpGet]
        public IActionResult View(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: /Notes/ToggleFavorite
        // Bir notun favori durumunu aç/kapa yapar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorite(int id, bool returnToFavorites = false)
        {
            var note = _noteRepository.GetById(id);
            if (note == null)
            {
                return NotFound();
            }

            note.IsFavorite = !note.IsFavorite;
            _noteRepository.Update(note);

            if (returnToFavorites)
            {
                return RedirectToAction(nameof(Favorites));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Notes/Favorites
        // Sadece favori notları listeleyen sayfa.
        [HttpGet]
        public IActionResult Favorites()
        {
            var notes = _noteRepository
                .GetAll()
                .Where(n => n.IsFavorite)
                .ToList();

            return View(notes);
        }

        // POST: /Notes/DeleteSelected
        // Checkbox ile seçilen Id'leri alıp siler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> selectedNoteIds)
        {
            if (selectedNoteIds != null && selectedNoteIds.Any())
            {
                _noteRepository.DeleteMany(selectedNoteIds);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

