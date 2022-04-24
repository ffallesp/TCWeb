using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TCWeb.Models;
using System.Web;

namespace TCWeb.Pages {
    /// <summary>
    /// Clase encargada de agregar el nuevo registro y comunicarse con la API
    /// </summary>
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;
        [BindProperty]
        public Item Candidato { get; set; }

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {

        }

        public async Task<ActionResult> OnPostAsync(Item candidato) {
            if (!ModelState.IsValid)
                return Page();

            HttpClient client = new HttpClient();
            var data = JsonConvert.SerializeObject(candidato);
            HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://ffalling-001-site1.itempurl.com/api/BBDD/PostCandidatosCloud", content);
            if (response.IsSuccessStatusCode) {
               
            }
            return Page();
        }
    }
}
