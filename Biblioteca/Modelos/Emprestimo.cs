using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Modelos
{
    // Classe Emprestimo
    public class Emprestimo
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdLivro { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public StatusEmprestimo Status { get; set; }
        public decimal MultaAplicada { get; set; }

        // Propriedades de navegação
        public Usuario Usuario { get; set; }
        public Livro Livro { get; set; }

        public Emprestimo(int idUsuario, int idLivro)
        {
            IdUsuario = idUsuario;
            IdLivro = idLivro;
            DataEmprestimo = DateTime.Now;
            DataPrevistaDevolucao = DateTime.Now.AddDays(5);
            Status = StatusEmprestimo.Ativo;
            MultaAplicada = 0;
        }

        public bool EstaAtrasado()
        {
            return DateTime.Now > DataPrevistaDevolucao && Status == StatusEmprestimo.Ativo;
        }

        public decimal CalcularMulta()
        {
            if (!EstaAtrasado()) return 0;

            int diasAtraso = (DateTime.Now - DataPrevistaDevolucao).Days;
            return diasAtraso * 1.0m; // 1 euro por dia
        }

        public void ExibirInformacoes()
        {
            string statusTexto = Status == StatusEmprestimo.Ativo ? "Ativo" :
                               Status == StatusEmprestimo.Devolvido ? "Devolvido" : "Atrasado";

            Console.WriteLine($"ID: {Id} | Usuário: {Usuario?.Nome} | Livro: {Livro?.Titulo}");
            Console.WriteLine($"Data Empréstimo: {DataEmprestimo:dd/MM/yyyy} | Devolução Prevista: {DataPrevistaDevolucao:dd/MM/yyyy}");
            Console.WriteLine($"Status: {statusTexto} | Multa: €{MultaAplicada:F2}");

            if (EstaAtrasado())
            {
                Console.WriteLine($"*** ATRASADO - Multa atual: €{CalcularMulta():F2} ***");
            }
        }
    }
}
