using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Infrastructure.Base;

namespace MailProxee.WebClient.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IMailDomainRepository _domains;
        private readonly IAliasRepository _aliases;

        public SignUpModel(IMailDomainRepository domains, IAliasRepository aliases)
        {
            _domains = domains;
            _aliases = aliases;
        }

        [BindProperty]
        public string Recipient { get; set; }

        [BindProperty]
        public string Domain { get; set; }

        [BindProperty]
        public Alias AssignedAlias { get; set; }

        public void OnGet()
        {

        }

        public async Task OnPost()
        {
            var domain = await _domains.Find(Domain);
            var alias = domain.AliasFrom(Recipient);
            AssignedAlias = await _aliases.Add(alias);
        }
    }
}