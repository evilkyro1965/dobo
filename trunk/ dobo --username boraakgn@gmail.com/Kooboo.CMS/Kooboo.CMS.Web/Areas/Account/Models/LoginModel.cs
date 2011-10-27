﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kooboo.CMS.Web.Areas.Account.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name="Username")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Required")]
        [UIHint("password")]
        public string Password { get; set; }
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}