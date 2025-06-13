using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{
    public class Livro
    {
        public int LivroID { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public int FaixaEtaria { get; set; }
        public bool Disponivel { get; set; } = true;
    }
}
