using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.Common
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
