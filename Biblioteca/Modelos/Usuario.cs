using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Enums;

namespace Biblioteca.Modelos
{
    // Classe Usuario herdando de Pessoa
    public class Usuario : Pessoa
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public TipoUsuario Tipo { get; set; }
        public int Idade { get; set; }
        public bool Ativo { get; set; }
        public decimal MultaTotal { get; set; }

        public Usuario(string nome, string email, string telefone, string username, string password, TipoUsuario tipo, int idade)
            : base(nome, email, telefone)
        {
            Username = username;
            PasswordHash = HashPassword(password);
            Tipo = tipo;
            Idade = idade;
            Ativo = true;
            MultaTotal = 0;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool VerificarPassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        public override void ExibirInformacoes()
        {
            Console.WriteLine($"ID: {Id} | Nome: {Nome} | Email: {Email} | Tipo: {Tipo} | Idade: {Idade} | Multa: €{MultaTotal:F2}");
        }
    }
}
