using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{

    public class Usuario : Pessoa
    {
        private string tipo;

        public int UsuarioID { get; set; }
        public override string Tipo => tipo;

        public Usuario(string tipo)
        {
            this.tipo = tipo;
        }

        public Usuario() { }
    }

}
