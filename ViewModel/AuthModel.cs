using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Вы не указали Email!")]
        //[DataType(DataType.EmailAddress, ErrorMessage = "Введен некорректный адрес электронной почты!")]
        //[EmailAddress(ErrorMessage = "Некорректный электронный адрес")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес электронной почты!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Вы не указали пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? ReturnURL { get; set; }
    }
    public class RegisterModel
    {
        [Required(ErrorMessage = "Вы не указали Email!")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный адрес электронной почты!")]
        //[Remote(action: "CheckEmail", controller: "Account", ErrorMessage = "Электронный адрес уже используется!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Вы не указали пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Вы не указали пароль!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно!")]
        public string ConfirmPassword { get; set; }
        public string ReturnURL { get; set; }
    }
}
