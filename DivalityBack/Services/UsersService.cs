using System;
using System.Security.Cryptography;
using System.Text;

namespace Divality.Services
{
    public class UsersService
    {
        public string HashPassword(String password)
        {
            byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);
            byte[] passwordBytesHash = new MD5CryptoServiceProvider().ComputeHash(passwordBytes);
            string passwordHash = Encoding.Default.GetString(passwordBytesHash);
            return passwordHash;
        }
    }
}