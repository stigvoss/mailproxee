using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Domain.Factories;
using Module.EmailProxy.Infrastructure.Base;

namespace MailProxee.WebClient.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly IAliasRepository _repository;

        public SignUpModel(IAliasRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public string Recipient { get; set; }

        [BindProperty]
        public Alias AssignedAlias { get; set; }

        public void OnGet()
        {

        }

        public async Task OnPost()
        {
            var alias = AliasFactory.AliasFrom(Recipient);
            AssignedAlias = await _repository.Add(alias);
        }
    }
}