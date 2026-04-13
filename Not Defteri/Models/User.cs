using System.ComponentModel.DataAnnotations;

namespace NotDefteriMvc.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanici adi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanici adi en fazla 50 karakter olabilir.")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public List<Note> Notes { get; set; } = [];
    }
}
