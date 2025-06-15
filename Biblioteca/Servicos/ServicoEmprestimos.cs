using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Repositorios;
using Biblioteca.Enums;
using Biblioteca.Modelos;

namespace Biblioteca.Servicos
{

    public class ServicoEmprestimos
    {
        private readonly RepositorioEmprestimos repoEmprestimos;
        private readonly RepositorioLivros repoLivros;
        private readonly RepositorioUsuarios repoUsuarios;

        public ServicoEmprestimos(RepositorioEmprestimos repoEmp, RepositorioLivros repoLiv, RepositorioUsuarios repoUsu)
        {
            repoEmprestimos = repoEmp;
            repoLivros = repoLiv;
            repoUsuarios = repoUsu;
        }

        public (bool sucesso, string mensagem) CriarEmprestimo(int idUsuario, int idLivro)
        {
            var usuario = repoUsuarios.ObterPorId(idUsuario);
            var livro = repoLivros.ObterPorId(idLivro);

            if (usuario == null) return (false, "Usuário não encontrado.");
            if (livro == null) return (false, "Livro não encontrado.");
            if (!livro.Disponivel) return (false, "Livro não disponível - já emprestado.");
            if (usuario.Idade < livro.IdadeMinima) return (false, $"Usuário não tem idade suficiente. Idade mínima: {livro.IdadeMinima}");

            try
            {
                var emprestimo = new Emprestimo(idUsuario, idLivro);
                repoEmprestimos.Inserir(emprestimo);

                // Marcar livro como indisponível
                livro.Disponivel = false;
                repoLivros.Atualizar(livro);

                return (true, "Empréstimo criado com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao criar empréstimo: {ex.Message}");
            }
        }

        public (bool sucesso, string mensagem, decimal multa) DevolverLivro(int idEmprestimo)
        {
            var emprestimo = repoEmprestimos.ObterPorId(idEmprestimo);
            if (emprestimo == null) return (false, "Empréstimo não encontrado.", 0);
            if (emprestimo.Status != StatusEmprestimo.Ativo) return (false, "Empréstimo já foi devolvido.", 0);

            try
            {
                // Calcular multa se necessário
                decimal multa = emprestimo.CalcularMulta();

                // Atualizar empréstimo
                emprestimo.DataDevolucao = DateTime.Now;
                emprestimo.Status = StatusEmprestimo.Devolvido;
                emprestimo.MultaAplicada = multa;
                repoEmprestimos.Atualizar(emprestimo);

                // Marcar livro como disponível
                var livro = repoLivros.ObterPorId(emprestimo.IdLivro);
                livro.Disponivel = true;
                repoLivros.Atualizar(livro);

                // Adicionar multa ao usuário se necessário
                if (multa > 0)
                {
                    var usuario = repoUsuarios.ObterPorId(emprestimo.IdUsuario);
                    usuario.MultaTotal += multa;
                    repoUsuarios.Atualizar(usuario);
                }

                string mensagem = multa > 0 ?
                    $"Livro devolvido com multa de {multa:F2} Euros por atraso." :
                    "Livro devolvido no prazo!";

                return (true, mensagem, multa);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao devolver livro: {ex.Message}", 0);
            }
        }

        public void AtualizarEmprestimosAtrasados()
        {
            var emprestimosAtrasados = repoEmprestimos.ObterAtrasados();
            foreach (var emp in emprestimosAtrasados)
            {
                if (emp.Status == StatusEmprestimo.Ativo)
                {
                    emp.Status = StatusEmprestimo.Atrasado;
                    repoEmprestimos.Atualizar(emp);
                }
            }
        }
    }
}
