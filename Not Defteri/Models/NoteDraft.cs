using System.ComponentModel.DataAnnotations;

namespace NotDefteriMvc.Models
{
    public class NoteDraft
    {
        [Required(ErrorMessage = "Baslik zorunludur.")]
        [StringLength(100, ErrorMessage = "Baslik en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Aciklama en fazla 2000 karakter olabilir.")]
        public string? Description { get; set; }
    }
}
