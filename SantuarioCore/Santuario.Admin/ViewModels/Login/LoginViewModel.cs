using System.ComponentModel.DataAnnotations;

namespace Santuario.Admin.ViewModels.Login
{
    public class LoginViewModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}