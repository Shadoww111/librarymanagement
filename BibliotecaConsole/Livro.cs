using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{
    public class Livro
    {
        private string _titulo;
        private string _autor;

        public int LivroID { get; set; }

        public string Titulo
        {
            get => _titulo;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Título inválido");
                _titulo = value;
            }
        }

        public string Autor
        {
            get => _autor;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Autor inválido");
                _autor = value;
            }
        }

        public int FaixaEtaria { get; set; }
        public bool Disponivel { get; set; } = true;

        // Construtor
        public Livro(string titulo, string autor, int faixaEtaria)
        {
            Titulo = titulo;     
            Autor = autor;
            FaixaEtaria = faixaEtaria;
            Disponivel = true;
        }
    }
}
