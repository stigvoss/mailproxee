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

            if (alias is null || Code is null)
            {
                Message = "Unable to activate.";
            }
            else if (alias.IsActivated)
            {
                Message = "Alias already activated.";
            }
            else
            {
                alias.Activate(Code);
                await _aliases.Update(alias);

                Message = "Alias activated.";
            }
        }
    }
}