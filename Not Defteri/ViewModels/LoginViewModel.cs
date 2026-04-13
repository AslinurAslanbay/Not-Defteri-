using System.ComponentModel.DataAnnotations;

namespace NotDefteriMvc.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanici adi zorunludur.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
