using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using TCWeb.Models;

namespace TCWeb.Pages
{
    /// <summary>
    /// Clase encargada de la autenticacion.
    /// </summary>
    public class LoginModel : PageModel {
        const string SessionUser = "_User";
        [BindProperty]
        public Usuario User { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync() {

            if (!ModelState.IsValid) {
                return Page();
            }
            using (StreamReader r = new StreamReader("secrets.json")) {
                string json = r.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Credenciales>(json);
                if (User.usuario == data.username && User.contrasena == data.password) {
                    HttpContext.Session.SetString(SessionUser, User.usuario);
                    return RedirectToPage("Index");
                } else {
                    ModelState.AddModelError("CustomError", "Usuario y/o contraseña invalidos.");//Error personalizado
                    return RedirectToPage("Login");
                }
            }
        }

        public ActionResult OnPostDelete() {
            HttpContext.Session.Clear();//Limpiar la sesión
            return RedirectToPage("Login");
        }
    }
}
