using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Repositorios;
using Biblioteca.Enums;
using Biblioteca.Modelos;
using Biblioteca.Data;
using Biblioteca.Servicos;
using System.Threading;

namespace Biblioteca.Sistema
{
    // Sistema Principal
    public class SistemaBiblioteca
    {
        private readonly DatabaseManager db;
        private readonly RepositorioUsuarios repoUsuarios;
        private readonly RepositorioLivros repoLivros;
        private readonly RepositorioEmprestimos repoEmprestimos;
        private readonly ServicoAutenticacao servicoAuth;
        private readonly ServicoEmprestimos servicoEmprestimos;
        private Usuario usuarioLogado;

        public SistemaBiblioteca(string connectionString)
        {
            db = new DatabaseManager(connectionString);
            repoUsuarios = new RepositorioUsuarios(db);
            repoLivros = new RepositorioLivros(db);
            repoEmprestimos = new RepositorioEmprestimos(db, repoUsuarios, repoLivros);
            servicoAuth = new ServicoAutenticacao(repoUsuarios);
            servicoEmprestimos = new ServicoEmprestimos(repoEmprestimos, repoLivros, repoUsuarios);

            CriarAdminPadrao();
        }

        private void CriarAdminPadrao()
        {
            try
            {
                var adminExistente = repoUsuarios.ObterPorUsername("admin");
                if (adminExistente == null)
                {
                    var admin = new Usuario("Administrador", "admin@biblioteca.com", "123456789",
                                          "admin", "admin123", TipoUsuario.Admin, 25);
                    repoUsuarios.Inserir(admin);
                    Console.WriteLine("Admin padrão criado: admin/admin123");
                }
            }
            catch { }
        }

        public void Iniciar()
        {
            Console.Clear();
   
            while (true)
            {
                if (usuarioLogado == null)
                {
                    MenuLoginRegistro();
                }
                else
                {
                    MostrarMenuPrincipal();
                }
            }
        }

        private void MenuLoginRegistro()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║     SISTEMA DE GESTÃO BIBLIOTECA     ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("\n═══ AUTENTICAÇÃO ═══");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Registrar novo usuário");
            Console.WriteLine("0. Sair");
            Console.Write("\nEscolha uma opção: ");

            switch (Console.ReadLine())
            {
                case "1":
                    FazerLogin();
                    break;
                case "2":
                    RegistrarUsuario();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }
        private string LerPasswordOculta()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }

       
        private void FazerLogin()
        {
            Console.Write("\nUsername: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = LerPasswordOculta();
            usuarioLogado = servicoAuth.Login(username, password);
            if (usuarioLogado != null)
            {
                Console.WriteLine($"\n✓ Login realizado com sucesso! Bem-vindo, {usuarioLogado.Nome}");
                servicoEmprestimos.AtualizarEmprestimosAtrasados(); // Atualizar status na entrada
            }
            else
            {
                Console.WriteLine("\n✗ Credenciais inválidas ou usuário inativo!");
            }
            Thread.Sleep(2000);
        }


        private void RegistrarUsuario()
        {
            Console.Clear();
            Console.WriteLine("\n═══ REGISTRO DE USUÁRIO ═══");
            Console.Write("Nome: ");
            string nome = Console.ReadLine();
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = LerPasswordOculta();
            Console.Write("Idade: ");
            int idade = int.Parse(Console.ReadLine());

            Console.WriteLine("\nTipo de usuário:");
            Console.WriteLine("1. Cliente");
            Console.WriteLine("2. Recepção");
            Console.WriteLine("3. Admin");
            Console.Write("Escolha: ");

            TipoUsuario tipo = (TipoUsuario)int.Parse(Console.ReadLine());

            bool sucesso = servicoAuth.RegistrarUsuario(nome, email, telefone, username, password, tipo, idade);

            if (sucesso)
            {
                Console.WriteLine("\n✓ Usuário registrado com sucesso!");
            }
            else
            {
                Console.WriteLine("\n✗ Erro ao registrar usuário. Username pode já existir.");
            }
            MenuContinuar();
        }

        private void MostrarMenuPrincipal()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║            MENU PRINCIPAL            ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine($"Ola, {usuarioLogado.Nome} ({usuarioLogado.Tipo})\n");

            switch (usuarioLogado.Tipo)
            {
                case TipoUsuario.Cliente:
                    MenuCliente();
                    break;
                case TipoUsuario.Rececao:
                    MenuRecepcao();
                    break;
                case TipoUsuario.Admin:
                    MenuAdmin();
                    break;
            }
        }

        private void MenuCliente()
        {
            Console.WriteLine("1. Ver meus empréstimos");
            Console.WriteLine("2. Listar livros disponíveis");
            Console.WriteLine("3. Pesquisar livros");
            Console.WriteLine("4. Ver minhas multas");
            Console.WriteLine("0. Logout");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListarEmprestimosUsuario(usuarioLogado.Id);
                    break;
                case "2":
                    ListarLivrosDisponiveis();
                    break;
                case "3":
                    PesquisarLivros();
                    break;
                case "4":
                    MostrarMultasUsuario();
                    break;
                case "0":
                    usuarioLogado = null;
                    Console.WriteLine("Logout realizado!");
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }

        private void MenuRecepcao()
        {
            Console.WriteLine("1. Criar empréstimo");
            Console.WriteLine("2. Devolver livro");
            Console.WriteLine("3. Listar empréstimos ativos");
            Console.WriteLine("4. Listar empréstimos atrasados");
            Console.WriteLine("5. Listar livros");
            Console.WriteLine("6. Pesquisar usuário");
            Console.WriteLine("7. Ver usuários com multas");
            Console.WriteLine("0. Logout");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    CriarEmprestimo();
                    break;
                case "2":
                    DevolverLivro();
                    break;
                case "3":
                    ListarEmprestimosAtivos();
                    break;
                case "4":
                    ListarEmprestimosAtrasados();
                    break;
                case "5":
                    MenuListarLivros();
                    break;
                case "6":
                    PesquisarUsuario();
                    break;
                case "7":
                    ListarUsuariosComMultas();
                    break;
                case "0":
                    usuarioLogado = null;
                    Console.WriteLine("Logout realizado!");
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }

        private void MenuAdmin()
        {
            Console.WriteLine("1. Gestão de Livros");
            Console.WriteLine("2. Gestão de Usuários");
            Console.WriteLine("3. Gestão de Empréstimos");
            Console.WriteLine("4. Relatórios");
            Console.WriteLine("5. POR FAZER- Ver estatísticas");
            Console.WriteLine("0. Logout");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    MenuGestaoLivros();
                    break;
                case "2":
                    MenuGestaoUsuarios();
                    break;
                case "3":
                    MenuGestaoEmprestimos();
                    break;
                case "4":
                    MenuRelatorios();
                    break;
                case "5":
                    //MostrarEstatisticas();
                    break;
                case "0":
                    usuarioLogado = null;
                    Console.WriteLine("Logout realizado!");
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }

        // Métodos de Gestão de Livros
        private void MenuGestaoLivros()
        {
            Console.Clear();
            Console.WriteLine("\n═══ GESTÃO DE LIVROS ═══");
            Console.WriteLine("1. Adicionar livro");
            Console.WriteLine("2. Editar livro");
            Console.WriteLine("3. Remover livro");
            Console.WriteLine("4. Listar todos os livros");
            Console.WriteLine("0. Voltar");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AdicionarLivro();
                    break;
                case "2":
                    EditarLivro();
                    break;
                case "3":
                    RemoverLivro();
                    break;
                case "4":
                    ListarTodosLivros();
                    break;
            }
        }

        private void AdicionarLivro()
        {
            Console.Clear();
            Console.WriteLine("\n═══ ADICIONAR LIVRO ═══");
            Console.Write("Título: ");
            string titulo = Console.ReadLine();
            Console.Write("Autor: ");
            string autor = Console.ReadLine();
            Console.Write("ISBN: ");
            string isbn = Console.ReadLine();
            Console.Write("Idade mínima: ");
            int idadeMinima = int.Parse(Console.ReadLine());
            Console.Write("Gênero: ");
            string genero = Console.ReadLine();

            var livro = new Livro(titulo, autor, isbn, idadeMinima, genero);
            repoLivros.Inserir(livro);

            Console.WriteLine("\n✓ Livro adicionado com sucesso!");
            MenuContinuar();
        }

        private void EditarLivro()
        {
            Console.Clear();
            Console.Write("\nID do livro para editar: ");
            int id = int.Parse(Console.ReadLine());

            var livro = repoLivros.ObterPorId(id);
            if (livro == null)
            {
                Console.WriteLine("Livro não encontrado!");
                return;
            }

            Console.WriteLine($"\nEditando: {livro.Titulo}");
            Console.Write($"Novo título [{livro.Titulo}]: ");
            string titulo = Console.ReadLine();
            if (!string.IsNullOrEmpty(titulo)) livro.Titulo = titulo;

            Console.Write($"Novo autor [{livro.Autor}]: ");
            string autor = Console.ReadLine();
            if (!string.IsNullOrEmpty(autor)) livro.Autor = autor;

            Console.Write($"Nova idade mínima [{livro.IdadeMinima}]: ");
            string idadeStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(idadeStr)) livro.IdadeMinima = int.Parse(idadeStr);

            repoLivros.Atualizar(livro);
            Console.WriteLine("\n✓ Livro atualizado com sucesso!");
            MenuContinuar();
        }

        private void RemoverLivro()
        {
            Console.Clear();
            Console.Write("\nID do livro para remover: ");
            int id = int.Parse(Console.ReadLine());

            var livro = repoLivros.ObterPorId(id);
            if (livro == null)
            {
                Console.WriteLine("Livro não encontrado!");
                return;
            }

            if (!livro.Disponivel)
            {
                Console.WriteLine("Não é possível remover um livro emprestado!");
                return;
            }

            Console.WriteLine($"Confirmar remoção do livro '{livro.Titulo}'? (s/N)");
            if (Console.ReadLine()?.ToLower() == "s")
            {
                repoLivros.Eliminar(id);
                Console.WriteLine("\n✓ Livro removido com sucesso!");
                MenuContinuar();
            }
        }

        // Métodos de Gestão de Usuários
        private void MenuGestaoUsuarios()
        {
            Console.Clear();
            Console.WriteLine("\n═══ GESTÃO DE USUÁRIOS ═══");
            Console.WriteLine("1. Listar todos os usuários");
            Console.WriteLine("2. Editar usuário");
            Console.WriteLine("3. Ativar/Desativar usuário");
            Console.WriteLine("4. Ver usuários com multas");
            Console.WriteLine("0. Voltar");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListarTodosUsuarios();
                    break;
                case "2":
                    EditarUsuario();
                    break;
                case "3":
                    ToggleUsuarioAtivo();
                    break;
                case "4":
                    ListarUsuariosComMultas();
                    break;
            }
        }

        private void EditarUsuario()
        {
            Console.Clear();
            Console.Write("\nID do usuário para editar: ");
            int id = int.Parse(Console.ReadLine());

            var usuario = repoUsuarios.ObterPorId(id);
            if (usuario == null)
            {
                Console.WriteLine("Usuário não encontrado!");
                return;
            }

            Console.WriteLine($"\nEditando: {usuario.Nome}");
            Console.Write($"Novo nome [{usuario.Nome}]: ");
            string nome = Console.ReadLine();
            if (!string.IsNullOrEmpty(nome)) usuario.Nome = nome;

            Console.Write($"Novo email [{usuario.Email}]: ");
            string email = Console.ReadLine();
            if (!string.IsNullOrEmpty(email)) usuario.Email = email;

            Console.Write($"Nova idade [{usuario.Idade}]: ");
            string idadeStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(idadeStr)) usuario.Idade = int.Parse(idadeStr);

            repoUsuarios.Atualizar(usuario);
            Console.WriteLine("\n✓ Usuário atualizado com sucesso!");
            MenuContinuar();
        }

        private void ToggleUsuarioAtivo()
        {
            Console.Clear();
            Console.Write("\nID do usuário: ");
            int id = int.Parse(Console.ReadLine());

            var usuario = repoUsuarios.ObterPorId(id);
            if (usuario == null)
            {
                Console.WriteLine("Usuário não encontrado!");
                return;
            }

            usuario.Ativo = !usuario.Ativo;
            repoUsuarios.Atualizar(usuario);

            string status = usuario.Ativo ? "ativado" : "desativado";
            Console.WriteLine($"\n✓ Usuário {status} com sucesso!");
            MenuContinuar();
        }

        // Métodos de Empréstimos
        private void CriarEmprestimo()
        {
            Console.Clear();
            Console.WriteLine("\n═══ CRIAR EMPRÉSTIMO ═══");
            Console.Write("ID do usuário: ");
            int idUsuario = int.Parse(Console.ReadLine());
            Console.Write("ID do livro: ");
            int idLivro = int.Parse(Console.ReadLine());

            var resultado = servicoEmprestimos.CriarEmprestimo(idUsuario, idLivro);

            if (resultado.sucesso)
            {
                Console.WriteLine($"\n✓ {resultado.mensagem}");
            }
            else
            {
                Console.WriteLine($"\n✗ {resultado.mensagem}");
            }
            MenuContinuar();
        }

        private void DevolverLivro()
        {
            Console.Clear();
            Console.WriteLine("\n═══ DEVOLVER LIVRO ═══");
            ListarEmprestimosAtivos();
            Console.Write("\nID do empréstimo: ");
            int idEmprestimo = int.Parse(Console.ReadLine());

            var resultado = servicoEmprestimos.DevolverLivro(idEmprestimo);

            if (resultado.sucesso)
            {
                Console.WriteLine($"\n✓ {resultado.mensagem}");
                if (resultado.multa > 0)
                {
                    Console.WriteLine($"Multa aplicada: {resultado.multa:F2} Euros");
                }
            }
            else
            {
                Console.WriteLine($"\n✗ {resultado.mensagem}");
            }
            MenuContinuar();
        }

        // Métodos de Listagem
        private void ListarTodosLivros()
        {
            Console.Clear();
            var livros = repoLivros.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS LIVROS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
            MenuContinuar();
        }

        private void ListarLivrosDisponiveis()
        {
            Console.Clear();
            var livros = repoLivros.ObterDisponiveis();
            Console.WriteLine("\n═══ LIVROS DISPONÍVEIS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
            MenuContinuar();
        }

        private void MenuListarLivros()
        {
            Console.Clear();
            Console.WriteLine("═══ LISTAR LIVROS ═══");
            Console.WriteLine("\n1. Livros disponíveis");
            Console.WriteLine("2. Livros emprestados");
            Console.WriteLine("3. Todos os livros");
            Console.Write("Escolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListarLivrosDisponiveis();
                    break;
                case "2":
                    ListarLivrosEmprestados();
                    break;
                case "3":
                    ListarTodosLivros();
                    break;
            }
        }

        private void ListarLivrosEmprestados()
        {
            Console.Clear();
            var livros = repoLivros.ObterEmprestados();
            Console.WriteLine("\n═══ LIVROS EMPRESTADOS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
            MenuContinuar();
        }

        private void ListarTodosUsuarios()
        {
            Console.Clear();
            var usuarios = repoUsuarios.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS USUÁRIOS ═══");
            foreach (var usuario in usuarios)
            {
                usuario.ExibirInformacoes();
            }
            MenuContinuar();
        }

        private void ListarUsuariosComMultas()
        {
            Console.Clear();
            var usuarios = repoUsuarios.ObterComMultas();
            Console.WriteLine("\n═══ USUÁRIOS COM MULTAS ═══");
            foreach (var usuario in usuarios)
            {
                usuario.ExibirInformacoes();
                var emprestimos = repoEmprestimos.ObterPorUsuario(usuario.Id);
                var emprestimosAtrasados = emprestimos.Where(e => e.EstaAtrasado()).ToList();
                if (emprestimosAtrasados.Any())
                {
                    Console.WriteLine("  Empréstimos atrasados:");
                    foreach (var emp in emprestimosAtrasados)
                    {
                        Console.WriteLine($"    - {emp.Livro.Titulo} (Atraso: {(DateTime.Now - emp.DataPrevistaDevolucao).Days} dias)");
                    }
                }
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void ListarEmprestimosAtivos()
        {
            Console.Clear();
            var emprestimos = repoEmprestimos.ObterAtivos();
            Console.WriteLine("\n═══ EMPRÉSTIMOS ATIVOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void ListarEmprestimosAtrasados()
        {
            Console.Clear();
            var emprestimos = repoEmprestimos.ObterAtrasados();
            Console.WriteLine("\n═══ EMPRÉSTIMOS ATRASADOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void ListarEmprestimosUsuario(int idUsuario)
        {
            Console.Clear();
            var emprestimos = repoEmprestimos.ObterPorUsuario(idUsuario);
            Console.WriteLine("\n═══ MEUS EMPRÉSTIMOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
            MenuContinuar();
        }

        // Métodos auxiliares
        private void PesquisarLivros()
        {
            Console.Clear();
            Console.Write("\nTermo de pesquisa (título ou autor): ");
            string termo = Console.ReadLine().ToLower();

            var livros = repoLivros.ObterTodos()
                .Where(l => l.Titulo.ToLower().Contains(termo) || l.Autor.ToLower().Contains(termo))
                .ToList();

            Console.WriteLine($"\n═══ RESULTADOS DA PESQUISA ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
            MenuContinuar();
        }

        private void PesquisarUsuario()
        {
            Console.Clear();

            Console.Write("\nTermo de pesquisa (nome ou email): ");
            string termo = Console.ReadLine().ToLower();

            var usuarios = repoUsuarios.ObterTodos()
                .Where(u => u.Nome.ToLower().Contains(termo) || u.Email.ToLower().Contains(termo))
                .ToList();

            Console.WriteLine($"\n═══ RESULTADOS DA PESQUISA ═══");
            foreach (var usuario in usuarios)
            {
                usuario.ExibirInformacoes();

                // Mostrar empréstimos ativos do usuário
                var emprestimosAtivos = repoEmprestimos.ObterPorUsuario(usuario.Id)
                    .Where(e => e.Status == StatusEmprestimo.Ativo).ToList();

                if (emprestimosAtivos.Any())
                {
                    Console.WriteLine("  Livros emprestados:");
                    foreach (var emp in emprestimosAtivos)
                    {
                        Console.WriteLine($"    - {emp.Livro.Titulo} (até {emp.DataPrevistaDevolucao:dd/MM/yyyy})");
                    }
                }
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void MostrarMultasUsuario()
        {
            Console.Clear();

            var usuario = repoUsuarios.ObterPorId(usuarioLogado.Id);
            Console.WriteLine($"\n═══ MINHAS MULTAS ═══");
            Console.WriteLine($"Total de multas: {usuario.MultaTotal:F2} Euros");

            var emprestimos = repoEmprestimos.ObterPorUsuario(usuarioLogado.Id);
            var comMulta = emprestimos.Where(e => e.MultaAplicada > 0).ToList();

            if (comMulta.Any())
            {
                Console.WriteLine("\nDetalhes das multas:");
                foreach (var emp in comMulta)
                {
                    Console.WriteLine($"- {emp.Livro.Titulo}: {emp.MultaAplicada:F2} Euros");
                }
            }
            MenuContinuar();
        }

        private void MenuGestaoEmprestimos()
        {
            Console.Clear();
            Console.WriteLine("\n═══ GESTÃO DE EMPRÉSTIMOS ═══");
            Console.WriteLine("1. Listar todos os empréstimos");
            Console.WriteLine("2. Empréstimos ativos");
            Console.WriteLine("3. Empréstimos atrasados");
            Console.WriteLine("4. Histórico de empréstimos");
            Console.WriteLine("0. Voltar");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListarTodosEmprestimos();
                    break;
                case "2":
                    ListarEmprestimosAtivos();
                    break;
                case "3":
                    ListarEmprestimosAtrasados();
                    break;
                case "4":
                    ListarHistoricoEmprestimos();
                    break;
            }
        }

        private void ListarTodosEmprestimos()
        {
            Console.Clear();

            var emprestimos = repoEmprestimos.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS EMPRÉSTIMOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void ListarHistoricoEmprestimos()
        {
            Console.Clear();

            var emprestimos = repoEmprestimos.ObterTodos()
                .Where(e => e.Status == StatusEmprestimo.Devolvido)
                .OrderByDescending(e => e.DataDevolucao)
                .ToList();

            Console.WriteLine("\n═══ HISTÓRICO DE EMPRÉSTIMOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
            MenuContinuar();
        }

        private void MenuRelatorios()
        {
            Console.Clear();

            Console.WriteLine("\n═══ RELATÓRIOS ═══");
            Console.WriteLine("1. Livros mais emprestados");
            Console.WriteLine("2. Usuários mais ativos");
            Console.WriteLine("3. Relatório de multas");
            Console.WriteLine("4. Empréstimos por período");
            Console.WriteLine("0. Voltar");
            Console.Write("\nEscolha: ");

            switch (Console.ReadLine())
            {
                case "1":
                    RelatorioLivrosMaisEmprestados();
                    break;
                case "2":
                    RelatorioUsuariosMaisAtivos();
                    break;
                case "3":
                    RelatorioMultas();
                    break;
                case "4":
                    RelatorioEmprestimosPorPeriodo();
                    break;
            }
        }

        private void RelatorioLivrosMaisEmprestados()
        {
            Console.Clear();

            var emprestimos = repoEmprestimos.ObterTodos();
            var estatisticas = emprestimos
                .GroupBy(e => e.IdLivro)
                .Select(g => new
                {
                    Livro = repoLivros.ObterPorId(g.Key),
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(10)
                .ToList();

            Console.WriteLine("\n═══ TOP 10 LIVROS MAIS EMPRESTADOS ═══");
            foreach (var stat in estatisticas)
            {
                Console.WriteLine($"{stat.Livro.Titulo} - {stat.Quantidade} empréstimos");
            }
            MenuContinuar();
        }

        private void RelatorioUsuariosMaisAtivos()
        {
            Console.Clear();

            var emprestimos = repoEmprestimos.ObterTodos();
            var estatisticas = emprestimos
                .GroupBy(e => e.IdUsuario)
                .Select(g => new
                {
                    Usuario = repoUsuarios.ObterPorId(g.Key),
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(10)
                .ToList();

            Console.WriteLine("\n═══ TOP 10 USUÁRIOS MAIS ATIVOS ═══");
            foreach (var stat in estatisticas)
            {
                Console.WriteLine($"{stat.Usuario.Nome} - {stat.Quantidade} empréstimos");
            }
            MenuContinuar();
        }

        private void RelatorioMultas()
        {
            Console.Clear();

            var usuarios = repoUsuarios.ObterComMultas();
            decimal totalMultas = usuarios.Sum(u => u.MultaTotal);

            Console.WriteLine("\n═══ RELATÓRIO DE MULTAS ═══");
            Console.WriteLine($"Total arrecadado em multas: {totalMultas:F2} Euros");
            Console.WriteLine($"Usuários com multas: {usuarios.Count}");
            Console.WriteLine("\nDetalhes:");
            foreach (var usuario in usuarios.Take(10))
            {
                Console.WriteLine($"{usuario.Nome}: {usuario.MultaTotal:F2} Euros");
            }
            MenuContinuar();
        }

        private void RelatorioEmprestimosPorPeriodo()
        {
            Console.Clear();

            Console.Write("\nData inicial (dd/MM/yyyy): ");
            DateTime dataInicial = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
            Console.Write("Data final (dd/MM/yyyy): ");
            DateTime dataFinal = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);

            var emprestimos = repoEmprestimos.ObterTodos()
                .Where(e => e.DataEmprestimo >= dataInicial && e.DataEmprestimo <= dataFinal)
                .ToList();

            Console.WriteLine($"\n═══ EMPRÉSTIMOS DE {dataInicial:dd/MM/yyyy} A {dataFinal:dd/MM/yyyy} ═══");
            Console.WriteLine($"Total de empréstimos: {emprestimos.Count}");
            Console.WriteLine($"Empréstimos ativos: {emprestimos.Count(e => e.Status == StatusEmprestimo.Ativo)}");
            Console.WriteLine($"Empréstimos devolvidos: {emprestimos.Count(e => e.Status == StatusEmprestimo.Devolvido)}");
            Console.WriteLine($"Empréstimos atrasados: {emprestimos.Count(e => e.Status == StatusEmprestimo.Atrasado)}");
            MenuContinuar();
        }
        private void MenuContinuar()
        {
            Console.WriteLine("\n═══ CLIQUE PARA VOLTAR ═══");
            Console.ReadLine();

        }
    }
}
