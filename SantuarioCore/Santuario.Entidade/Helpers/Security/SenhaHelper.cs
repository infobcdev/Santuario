using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Santuario.Negocio.Helpers.Security
{
    public static class SenhaHelper
    {
        private const int Iteracoes = 10000;
        private const int TamanhoSalt = 16; // 128 bits
        private const int TamanhoHash = 32; // 256 bits

        // =========================================
        // 1️⃣ GERAR HASH + SALT (byte[])
        // =========================================
        public static (byte[] hash, byte[] salt) CriarSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("Senha inválida.", nameof(senha));

            var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);

            var hash = KeyDerivation.Pbkdf2(
                password: senha,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iteracoes,
                numBytesRequested: TamanhoHash);

            return (hash, salt);
        }

        // =========================================
        // 2️⃣ VERIFICAR SENHA (LOGIN)
        // =========================================
        public static bool VerificarSenha(string senhaDigitada, byte[] hashArmazenado, byte[] saltArmazenado)
        {
            if (string.IsNullOrWhiteSpace(senhaDigitada)) return false;
            if (hashArmazenado == null || hashArmazenado.Length == 0) return false;
            if (saltArmazenado == null || saltArmazenado.Length == 0) return false;

            var hashDigitado = KeyDerivation.Pbkdf2(
                password: senhaDigitada,
                salt: saltArmazenado,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iteracoes,
                numBytesRequested: hashArmazenado.Length);

            return CryptographicOperations.FixedTimeEquals(hashDigitado, hashArmazenado);
        }
    }
}