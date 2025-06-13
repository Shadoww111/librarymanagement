
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
            Console.WriteLine("(Funcionalidade em desenvolvimento ou repetida de versões anteriores)");
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
                UtilizadorAtual = new Usuario
                {
                    UsuarioID = (int)reader["UsuarioID"],
                    Nome = reader["Nome"].ToString(),
                    Idade = (int)reader["Idade"],
                    Tipo = reader["Tipo"].ToString()
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
}
