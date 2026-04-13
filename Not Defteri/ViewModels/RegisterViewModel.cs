using System.ComponentModel.DataAnnotations;

namespace NotDefteriMvc.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanici adi zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanici adi en fazla 50 karakter olabilir.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre zorunludur.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Sifre 4 ile 100 karakter arasinda olmalidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre tekrari zorunludur.")]
        [Compare(nameof(Password), ErrorMessage = "Sifreler ayni degil.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
