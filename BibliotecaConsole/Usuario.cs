using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{
 
      
        public class Usuario
        {
            public int UsuarioID { get; set; }
            public string Nome { get; set; }
            public int Idade { get; set; }
            public string Tipo { get; set; } // Cliente, Rececao, Admin
        }
    
}
