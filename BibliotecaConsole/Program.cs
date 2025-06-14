
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using BibliotecaConsole;
class Program
{
    static Usuario UtilizadorAtual = null;

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Sistema de Biblioteca ===");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Registar novo utilizador");
            Console.WriteLine("3. Sair");
            Console.Write("Escolha uma opção: ");
            string escolha = Console.ReadLine();

            if (escolha == "1" && FazerLogin()) break;
            else if (escolha == "2") RegistarUtilizador();
            else if (escolha == "3") return;
            else Console.WriteLine("Opção inválida!");
            Console.ReadKey();
        }

        // Menu principal após login bem-sucedido
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Bem-vindo, {UtilizadorAtual.Nome} ({UtilizadorAtual.Tipo}) ===");
            Console.WriteLine("1. Listar livros");
            Console.WriteLine("2. Listar utilizadores");
            Console.WriteLine("3. Ver multas");
            if (UtilizadorAtual.Tipo == "Admin")
            {
                Console.WriteLine("4. Adicionar livro");
                Console.WriteLine("5. Adicionar utilizador");
                Console.WriteLine("6. Criar empréstimo");
                Console.WriteLine("7. Devolver livro");
                Console.WriteLine("8. Listar livros emprestados");
            }
            else if (UtilizadorAtual.Tipo == "Rececao")
            {
                Console.WriteLine("4. Criar empréstimo");
                Console.WriteLine("5. Devolver livro");
                Console.WriteLine("6. Listar livros emprestados");
            }
            Console.WriteLine("0. Terminar sessão");
            Console.Write("Escolha uma opção: ");

            string opcao = Console.ReadLine();
            if (opcao == "0") break;
            switch (opcao)
            {
                case "1": ListarLivros(); break;
                case "2": ListarUtilizadores(); break;
                case "3": VerMultas(); break;
                case "4":
                    if (UtilizadorAtual.Tipo == "Admin") AdicionarLivro();
                    else if (UtilizadorAtual.Tipo == "Rececao") CriarEmprestimo();
                    break;
                case "5":
                    if (UtilizadorAtual.Tipo == "Admin") AdicionarUtilizador();
                    else if (UtilizadorAtual.Tipo == "Rececao") DevolverLivro();
                    break;
                case "6":
                    if (UtilizadorAtual.Tipo == "Admin") CriarEmprestimo();
                    else if (UtilizadorAtual.Tipo == "Rececao") ListarLivrosEmprestados();
                    break;
                case "7":
                    if (UtilizadorAtual.Tipo == "Admin") DevolverLivro();
                    break;
                case "8":
                    if (UtilizadorAtual.Tipo == "Admin") ListarLivrosEmprestados();
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
            Console.ReadKey();
            Console.ReadKey();
        }
    }

    static bool FazerLogin()
    {
        Console.Clear();
        Console.WriteLine("=== Login ===");
        Console.Write("Nome: ");
        string nome = Console.ReadLine();
        Console.Write("Password: ");
        string pass = Console.ReadLine();

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Usuarios WHERE Nome = @nome AND Password = @pass", conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@pass", pass);

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UtilizadorAtual = new Usuario(reader["Tipo"].ToString())
                {
                    UsuarioID = (int)reader["UsuarioID"],
                    Nome = reader["Nome"].ToString(),
                    Idade = (int)reader["Idade"]
                };
                return true;
            }
        }

        Console.WriteLine("Credenciais inválidas!");
        return false;
    }

    static void RegistarUtilizador()
    {
        Console.Clear();
        Console.WriteLine("=== Registo de Novo Utilizador ===");
        Console.Write("Nome: ");
        string nome = Console.ReadLine();
        Console.Write("Idade: ");
        int idade = int.Parse(Console.ReadLine());
        Console.Write("Tipo (Cliente/Rececao/Admin): ");
        string tipo = Console.ReadLine();
        Console.Write("Password: ");    
        string pass = Console.ReadLine();

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Usuarios (Nome, Idade, Tipo, Password) VALUES (@nome, @idade, @tipo, @pass)", conn);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@idade", idade);
            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@pass", pass);
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("Utilizador registado com sucesso!");
    }
    static void ListarLivros()
    {
        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Livros", conn);
            var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Lista de Livros ---");
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["LivroID"]} | {reader["Titulo"]} - {reader["Autor"]} | Faixa Etária: {reader["FaixaEtaria"]} | {(Convert.ToBoolean(reader["Disponivel"]) ? "Disponível" : "Emprestado")}");
            }
        }
    }

    static void ListarUtilizadores()
    {
        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Usuarios", conn);
            var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Lista de Utilizadores ---");
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["UsuarioID"]} | {reader["Nome"]} | Idade: {reader["Idade"]} | Tipo: {reader["Tipo"]}");
            }
        }
    }

    static void VerMultas()
    {
        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            SqlCommand cmd;
            if (UtilizadorAtual.Tipo == "Cliente")
            {
                cmd = new SqlCommand("SELECT DATEDIFF(day, DataEmprestimo, GETDATE()) - 5 AS DiasAtraso FROM Emprestimos WHERE UsuarioID = @uid AND DataDevolucao IS NULL AND DATEDIFF(day, DataEmprestimo, GETDATE()) > 5", conn);
                cmd.Parameters.AddWithValue("@uid", UtilizadorAtual.UsuarioID);
            }
            else
            {
                cmd = new SqlCommand("SELECT u.Nome, DATEDIFF(day, e.DataEmprestimo, GETDATE()) - 5 AS DiasAtraso FROM Emprestimos e JOIN Usuarios u ON e.UsuarioID = u.UsuarioID WHERE e.DataDevolucao IS NULL AND DATEDIFF(day, e.DataEmprestimo, GETDATE()) > 5", conn);
            }
            var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Multas ---");
            while (reader.Read())
            {
                if (UtilizadorAtual.Tipo == "Cliente")
                    Console.WriteLine($"Multa atual: {reader["DiasAtraso"]} Euros");    
                else
                    Console.WriteLine($"{reader["Nome"]} - Multa: {reader["DiasAtraso"]} Euros");
            }
        }
    }

    static void AdicionarLivro()
    {
        Console.Write("Título: ");
        string titulo = Console.ReadLine();
        Console.Write("Autor: ");
        string autor = Console.ReadLine();
        Console.Write("Faixa Etária: ");
        int faixa = int.Parse(Console.ReadLine());

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Livros (Titulo, Autor, FaixaEtaria, Disponivel) VALUES (@t, @a, @f, 1)", conn);
            cmd.Parameters.AddWithValue("@t", titulo);
            cmd.Parameters.AddWithValue("@a", autor);
            cmd.Parameters.AddWithValue("@f", faixa);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Livro adicionado com sucesso!");
    }

    static void AdicionarUtilizador()
    {
        Console.Write("Nome: ");
        string nome = Console.ReadLine();
        Console.Write("Idade: ");
        int idade = int.Parse(Console.ReadLine());
        Console.Write("Tipo (Cliente/Rececao/Admin): ");
        string tipo = Console.ReadLine();
        Console.Write("Password: ");
        string pass = Console.ReadLine();

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Usuarios (Nome, Idade, Tipo, Password) VALUES (@n, @i, @t, @p)", conn);
            cmd.Parameters.AddWithValue("@n", nome);
            cmd.Parameters.AddWithValue("@i", idade);
            cmd.Parameters.AddWithValue("@t", tipo);
            cmd.Parameters.AddWithValue("@p", pass);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Utilizador adicionado com sucesso!");
    }

    static void CriarEmprestimo()
    {
        Console.Write("ID do Livro: ");
        int livroId = int.Parse(Console.ReadLine());
        Console.Write("ID do Utilizador: ");
        int usuarioId = int.Parse(Console.ReadLine());

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();

            var checkLivro = new SqlCommand("SELECT FaixaEtaria, Disponivel FROM Livros WHERE LivroID = @id", conn);
            checkLivro.Parameters.AddWithValue("@id", livroId);
            var reader = checkLivro.ExecuteReader();
            if (!reader.Read()) { Console.WriteLine("Livro não encontrado!"); return; }
            int faixa = (int)reader["FaixaEtaria"];
            bool disponivel = (bool)reader["Disponivel"];
            reader.Close();

            var checkUser = new SqlCommand("SELECT Idade FROM Usuarios WHERE UsuarioID = @id", conn);
            checkUser.Parameters.AddWithValue("@id", usuarioId);
            var idade = (int?)checkUser.ExecuteScalar();
            if (idade == null) { Console.WriteLine("Utilizador não encontrado!"); return; }
            if (!disponivel) { Console.WriteLine("Livro indisponível!"); return; }
            if (idade < faixa) { Console.WriteLine("Utilizador não tem idade suficiente!"); return; }

            var insert = new SqlCommand("INSERT INTO Emprestimos (LivroID, UsuarioID, DataEmprestimo) VALUES (@l, @u, GETDATE()); UPDATE Livros SET Disponivel = 0 WHERE LivroID = @l", conn);
            insert.Parameters.AddWithValue("@l", livroId);
            insert.Parameters.AddWithValue("@u", usuarioId);
            insert.ExecuteNonQuery();
        }
        Console.WriteLine("Empréstimo criado com sucesso!");
    }

    static void DevolverLivro()
    {
        Console.Write("ID do Empréstimo: ");
        int id = int.Parse(Console.ReadLine());

        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT LivroID, DataEmprestimo FROM Emprestimos WHERE EmprestimoID = @id AND DataDevolucao IS NULL", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var reader = cmd.ExecuteReader();
            if (!reader.Read()) { Console.WriteLine("Empréstimo não encontrado ou já devolvido!"); return; }
            int livroId = (int)reader["LivroID"];
            DateTime data = (DateTime)reader["DataEmprestimo"];
            int dias = (DateTime.Now - data).Days - 5;
            reader.Close();

            var devolver = new SqlCommand("UPDATE Emprestimos SET DataDevolucao = GETDATE() WHERE EmprestimoID = @id; UPDATE Livros SET Disponivel = 1 WHERE LivroID = @livro", conn);
            devolver.Parameters.AddWithValue("@id", id);
            devolver.Parameters.AddWithValue("@livro", livroId);
            devolver.ExecuteNonQuery();

            if (dias > 0) Console.WriteLine($"Multa a pagar: {dias} Euros");
            Console.WriteLine("Livro devolvido com sucesso!");
        }
    }

    static void ListarLivrosEmprestados()
    {
        using (var conn = new SqlConnection(ConexaoBD.StringConexao))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT e.EmprestimoID, l.Titulo, u.Nome FROM Emprestimos e JOIN Livros l ON e.LivroID = l.LivroID JOIN Usuarios u ON e.UsuarioID = u.UsuarioID WHERE e.DataDevolucao IS NULL", conn);
            var reader = cmd.ExecuteReader();
            Console.WriteLine("\n--- Livros Emprestados ---");
            while (reader.Read())
            {
                Console.WriteLine($"Empréstimo {reader["EmprestimoID"]}: '{reader["Titulo"]}' emprestado a {reader["Nome"]}");
            }
        }
    }

}
