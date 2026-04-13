using System.ComponentModel.DataAnnotations;

namespace NotDefteriMvc.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Kullanici adi zorunludur.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni sifre zorunludur.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Sifre 4 ile 100 karakter arasinda olmalidir.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre tekrari zorunludur.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Sifreler ayni degil.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
