using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotDefteriMvc.Models;
using NotDefteriMvc.Services;
using NotDefteriMvc.ViewModels;

namespace NotDefteriMvc.Controllers
{
    public class NotesController : Controller
    {
        public const string DraftSessionKey = "PendingNoteDraft";

        private readonly INoteRepository _noteRepository;

        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(bool selectMode = false, string? search = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var model = new NotesIndexViewModel
            {
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                SelectMode = selectMode,
                SearchQuery = search,
                StartDate = startDate?.ToString("yyyy-MM-dd"),
                EndDate = endDate?.ToString("yyyy-MM-dd"),
                CurrentUserId = GetCurrentUserId(),
                CurrentUserName = User.Identity?.Name
            };

            if (!model.IsAuthenticated || model.CurrentUserId == null)
            {
                model.Draft = LoadDraft();
                return View(model);
            }

            var notes = _noteRepository.GetAll(model.CurrentUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                notes = notes.Where(n =>
                    (n.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (n.Description?.ToLower().Contains(searchLower) ?? false))
                    .ToList();
            }

            if (startDate.HasValue)
            {
                notes = notes.Where(n => n.CreatedDate.Date >= startDate.Value.Date).ToList();
            }

            if (endDate.HasValue)
            {
                notes = notes.Where(n => n.CreatedDate.Date <= endDate.Value.Date).ToList();
            }

            model.Notes = notes;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Create()
        {
            var note = new Note();
            var draft = LoadDraft();
            if (!string.IsNullOrWhiteSpace(draft.Title) || !string.IsNullOrWhiteSpace(draft.Description))
            {
                note.Title = draft.Title;
                note.Description = draft.Description;
                ClearDraft();
            }

            return View(note);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NoteDraft draft)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                SaveDraft(draft);
                TempData["AuthMessage"] = "Notu kaydetmek icin once giris yapmaniz veya kayit olmaniz gerekiyor.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(new Note
                {
                    Title = draft.Title,
                    Description = draft.Description
                });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            _noteRepository.Add(new Note
            {
                Title = draft.Title.Trim(),
                Description = draft.Description,
                UserId = userId.Value
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var note = _noteRepository.GetById(id, userId.Value);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Note note)
        {
            if (!ModelState.IsValid)
            {
                return View(note);
            }

            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            note.UserId = userId.Value;
            _noteRepository.Update(note);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public IActionResult View(int id)
        {
            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var note = _noteRepository.GetById(id, userId.Value);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorite(int id, bool returnToFavorites = false)
        {
            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var note = _noteRepository.GetById(id, userId.Value);
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

        [HttpGet]
        [Authorize]
        public IActionResult Favorites()
        {
            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notes = _noteRepository
                .GetAll(userId.Value)
                .Where(n => n.IsFavorite)
                .ToList();

            return View(notes);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> selectedNoteIds)
        {
            var userId = GetRequiredUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (selectedNoteIds != null && selectedNoteIds.Any())
            {
                _noteRepository.DeleteMany(selectedNoteIds, userId.Value);
            }

            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private int? GetRequiredUserId()
        {
            return User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
        }

        private void SaveDraft(NoteDraft draft)
        {
            var serializedDraft = JsonSerializer.Serialize(draft);
            HttpContext.Session.SetString(DraftSessionKey, serializedDraft);
        }

        private NoteDraft LoadDraft()
        {
            var serializedDraft = HttpContext.Session.GetString(DraftSessionKey);
            if (string.IsNullOrWhiteSpace(serializedDraft))
            {
                return new NoteDraft();
            }

            return JsonSerializer.Deserialize<NoteDraft>(serializedDraft) ?? new NoteDraft();
        }

        private void ClearDraft()
        {
            HttpContext.Session.Remove(DraftSessionKey);
        }
    }
}
