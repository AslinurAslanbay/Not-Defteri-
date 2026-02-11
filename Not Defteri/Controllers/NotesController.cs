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
        [HttpGet]
        public IActionResult Index(bool selectMode = false)
        {
            var notes = _noteRepository.GetAll();

            // ViewBag ile View'a ek veriler gönderebiliriz.
            ViewBag.SelectMode = selectMode;

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

