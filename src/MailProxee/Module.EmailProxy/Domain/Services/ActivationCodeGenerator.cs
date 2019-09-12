using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Module.EmailProxy.Domain.Services
{
    public class ActivationCodeGenerator
    {
        public string GenerateCode()
        {
            var randomBytes = new byte[16];
            var randomNumberGenerator = new RNGCryptoServiceProvider();

            randomNumberGenerator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
