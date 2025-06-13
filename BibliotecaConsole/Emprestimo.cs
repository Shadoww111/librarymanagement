using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaConsole
{
    public class Emprestimo
    {
        public int EmprestimoID { get; set; }
        public int LivroID { get; set; }
        public int UsuarioID { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime? DataDevolucao { get; set; }

        public int DiasAtraso => DataDevolucao.HasValue && (DataDevolucao.Value - DataEmprestimo).Days > 5
            ? (DataDevolucao.Value - DataEmprestimo).Days - 5 : 0;

        public decimal Multa => DiasAtraso * 1.0m;
    }
}
