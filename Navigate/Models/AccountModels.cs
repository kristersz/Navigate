using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace Navigate.Models
{

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Pašreizējā parole")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Jaunā parole")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Apstipriniet jauno paroli")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }      
    }

    public class UserDataModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-pasta adrese")]
        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        public string Email { get; set; }

        [Display(Name = "Pamata atrašanās vieta")]
        public string BaseLocation { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        [Display(Name = "Lietotājvārds")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        [DataType(DataType.Password)]
        [Display(Name = "Parole")]
        public string Password { get; set; }

        [Display(Name = "Atcerēties mani")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        [Display(Name = "Lietotājvārds")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        [StringLength(100, ErrorMessage = "Parolei jābūt vismaz {2} simbolus garai!", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Parole")]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Apstipriniet paroli")]
        [Compare("Password", ErrorMessage = "Parolei un apstiprinājuma parolei ir jāsakrīt!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Lauks {0} ir obligāts")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-pasta adrese")]
        public string Email { get; set; }

        [Display(Name = "Pamata atrašanās vieta")]
        public string BaseLocation { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
