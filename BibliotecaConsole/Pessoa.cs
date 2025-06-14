using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{
    public abstract class Pessoa
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public int Idade { get; set; }
        public abstract string Tipo { get; }

        public virtual void MostrarInfo()
        {
            Console.WriteLine($"{Nome} ({Tipo}) - {Idade} anos");
        }
    }

    public class Cliente : Pessoa
    {
        public override string Tipo => "Cliente";
    }

    public class Rececao : Pessoa
    {
        public override string Tipo => "Rececao";
    }

    public class Admin : Pessoa
    {
        public override string Tipo => "Admin";
    }
}
