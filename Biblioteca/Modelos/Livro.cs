using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Modelos
{
    // Classe Livro
    public class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public int IdadeMinima { get; set; }
        public bool Disponivel { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Genero { get; set; }

        public Livro(string titulo, string autor, string isbn, int idadeMinima, string genero)
        {
            Titulo = titulo;
            Autor = autor;
            ISBN = isbn;
            IdadeMinima = idadeMinima;
            Genero = genero;
            Disponivel = true;
            DataCriacao = DateTime.Now;
        }

        public void ExibirInformacoes()
        {
            string status = Disponivel ? "Disponível" : "Emprestado";
            Console.WriteLine($"ID: {Id} | Título: {Titulo} | Autor: {Autor} | Idade Mín: {IdadeMinima} | Status: {status}");
        }
    }
}
