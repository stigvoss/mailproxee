using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Module.EmailProxy.Infrastructure.Base;

namespace MailProxee.WebClient.Pages
{
    public class ActivationModel : PageModel
    {
        private readonly IAliasRepository _aliases;

        public ActivationModel(IAliasRepository aliases)
        {
            _aliases = aliases;
        }

        [BindProperty(Name = "Alias", SupportsGet = true)]
        public Guid AliasId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        public string Message { get; set; }

        public async Task OnGet()
        {
            var alias = await _aliases.Find(AliasId);

            if (alias?.ActivationCriteria?.IsActivated ?? false)
            {
                Message = "Alias already activated.";
            }
            else if (alias?.ActivationCriteria?.ActivationCode == Code)
            {
                Message = "Alias activated.";
            }
            else
            {
                Message = "Unable to activate.";
            }
        }
    }
}