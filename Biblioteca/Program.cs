using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SistemaGestaBiblioteca
{

    // Enums
    public enum TipoUsuario
    {
        Cliente = 1,
        Recepcao = 2,
        Admin = 3
    }

    public enum StatusEmprestimo
    {
        Ativo = 1,
        Devolvido = 2,
        Atrasado = 3
    }

    // Classe base abstrata para Pessoa
    public abstract class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateTime DataCriacao { get; set; }

        protected Pessoa(string nome, string email, string telefone)
        {
            Nome = nome;
            Email = email;
            Telefone = telefone;
            DataCriacao = DateTime.Now;
        }

        public abstract void ExibirInformacoes();
    }

    // Classe Usuario herdando de Pessoa
    public class Usuario : Pessoa
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public TipoUsuario Tipo { get; set; }
        public int Idade { get; set; }
        public bool Ativo { get; set; }
        public decimal MultaTotal { get; set; }

        public Usuario(string nome, string email, string telefone, string username, string password, TipoUsuario tipo, int idade)
            : base(nome, email, telefone)
        {
            Username = username;
            PasswordHash = HashPassword(password);
            Tipo = tipo;
            Idade = idade;
            Ativo = true;
            MultaTotal = 0;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool VerificarPassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        public override void ExibirInformacoes()
        {
            Console.WriteLine($"ID: {Id} | Nome: {Nome} | Email: {Email} | Tipo: {Tipo} | Idade: {Idade} | Multa: €{MultaTotal:F2}");
        }
    }

    // Classe Livro
    public class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public int IdadeMinima { get; set; }
        public bool Disponivel { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Genero { get; set; }

        public Livro(string titulo, string autor, string isbn, int idadeMinima, string genero)
        {
            Titulo = titulo;
            Autor = autor;
            ISBN = isbn;
            IdadeMinima = idadeMinima;
            Genero = genero;
            Disponivel = true;
            DataCriacao = DateTime.Now;
        }

        public void ExibirInformacoes()
        {
            string status = Disponivel ? "Disponível" : "Emprestado";
            Console.WriteLine($"ID: {Id} | Título: {Titulo} | Autor: {Autor} | Idade Mín: {IdadeMinima} | Status: {status}");
        }
    }

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

    // Classe para gestão da base de dados
    public class DatabaseManager
    {
        private readonly string connectionString;
        private readonly string nomeBaseDados = "BibliotecaDB";

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;

            CriarBaseDeDadosSeNaoExistir();
            CriarTabelasSeNaoExistirem();
        }

        private void CriarBaseDeDadosSeNaoExistir()
        {
            // Altera a connection string para ligar-se à base master
            var connectionStringMaster = connectionString.Replace($"Database={nomeBaseDados}", "Database=master");

            using (SqlConnection connection = new SqlConnection(connectionStringMaster))
            {
                connection.Open();
                string query = $@"IF DB_ID('{nomeBaseDados}') IS NULL
                              BEGIN
                                  CREATE DATABASE [{nomeBaseDados}];
                              END";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CriarTabelasSeNaoExistirem()
        {
            string[] scripts = {
            @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
              CREATE TABLE Usuarios (
                  Id int IDENTITY(1,1) PRIMARY KEY,
                  Nome nvarchar(100) NOT NULL,
                  Email nvarchar(100) NOT NULL UNIQUE,
                  Telefone nvarchar(20),
                  Username nvarchar(50) NOT NULL UNIQUE,
                  PasswordHash nvarchar(255) NOT NULL,
                  Tipo int NOT NULL,
                  Idade int NOT NULL,
                  Ativo bit NOT NULL DEFAULT 1,
                  MultaTotal decimal(10,2) DEFAULT 0,
                  DataCriacao datetime DEFAULT GETDATE()
              )",

            @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Livros' AND xtype='U')
              CREATE TABLE Livros (
                  Id int IDENTITY(1,1) PRIMARY KEY,
                  Titulo nvarchar(200) NOT NULL,
                  Autor nvarchar(100) NOT NULL,
                  ISBN nvarchar(20) UNIQUE,
                  IdadeMinima int DEFAULT 0,
                  Disponivel bit NOT NULL DEFAULT 1,
                  Genero nvarchar(50),
                  DataCriacao datetime DEFAULT GETDATE()
              )",

            @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Emprestimos' AND xtype='U')
              CREATE TABLE Emprestimos (
                  Id int IDENTITY(1,1) PRIMARY KEY,
                  IdUsuario int NOT NULL,
                  IdLivro int NOT NULL,
                  DataEmprestimo datetime NOT NULL DEFAULT GETDATE(),
                  DataPrevistaDevolucao datetime NOT NULL,
                  DataDevolucao datetime NULL,
                  Status int NOT NULL DEFAULT 1,
                  MultaAplicada decimal(10,2) DEFAULT 0,
                  FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
                  FOREIGN KEY (IdLivro) REFERENCES Livros(Id)
              )"
        };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (string script in scripts)
                {
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public void ExecutarComando(string sql, params SqlParameter[] parametros)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);
                    command.ExecuteNonQuery();
                }
            }
        }

        public T ExecutarScalar<T>(string sql, params SqlParameter[] parametros)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);
                    object resultado = command.ExecuteScalar();
                    return resultado == DBNull.Value ? default(T) : (T)resultado;
                }
            }
        }

        public List<T> ExecutarQuery<T>(string sql, Func<SqlDataReader, T> mapear, params SqlParameter[] parametros)
        {
            List<T> resultados = new List<T>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parametros != null)
                        command.Parameters.AddRange(parametros);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(mapear(reader));
                        }
                    }
                }
            }
            return resultados;
        }
    }

    // Repositório base genérico
    public abstract class RepositorioBase<T>
    {
        protected readonly DatabaseManager db;

        protected RepositorioBase(DatabaseManager database)
        {
            db = database;
        }

        public abstract void Inserir(T entidade);
        public abstract void Atualizar(T entidade);
        public abstract void Eliminar(int id);
        public abstract T ObterPorId(int id);
        public abstract List<T> ObterTodos();
    }

    // Repositório de Usuários
    public class RepositorioUsuarios : RepositorioBase<Usuario>
    {
        public RepositorioUsuarios(DatabaseManager database) : base(database) { }

        public override void Inserir(Usuario usuario)
        {
            string sql = @"INSERT INTO Usuarios (Nome, Email, Telefone, Username, PasswordHash, Tipo, Idade, Ativo, MultaTotal, DataCriacao)
                          OUTPUT INSERTED.Id
                          VALUES (@Nome, @Email, @Telefone, @Username, @PasswordHash, @Tipo, @Idade, @Ativo, @MultaTotal, @DataCriacao)";

            usuario.Id = db.ExecutarScalar<int>(sql,
                new SqlParameter("@Nome", usuario.Nome),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@Telefone", usuario.Telefone ?? (object)DBNull.Value),
                new SqlParameter("@Username", usuario.Username),
                new SqlParameter("@PasswordHash", usuario.PasswordHash),
                new SqlParameter("@Tipo", (int)usuario.Tipo),
                new SqlParameter("@Idade", usuario.Idade),
                new SqlParameter("@Ativo", usuario.Ativo),
                new SqlParameter("@MultaTotal", usuario.MultaTotal),
                new SqlParameter("@DataCriacao", usuario.DataCriacao)
            );
        }

        public override void Atualizar(Usuario usuario)
        {
            string sql = @"UPDATE Usuarios SET Nome = @Nome, Email = @Email, Telefone = @Telefone, 
                          Idade = @Idade, Ativo = @Ativo, MultaTotal = @MultaTotal
                          WHERE Id = @Id";

            db.ExecutarComando(sql,
                new SqlParameter("@Id", usuario.Id),
                new SqlParameter("@Nome", usuario.Nome),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@Telefone", usuario.Telefone ?? (object)DBNull.Value),
                new SqlParameter("@Idade", usuario.Idade),
                new SqlParameter("@Ativo", usuario.Ativo),
                new SqlParameter("@MultaTotal", usuario.MultaTotal)
            );
        }

        public override void Eliminar(int id)
        {
            string sql = "DELETE FROM Usuarios WHERE Id = @Id";
            db.ExecutarComando(sql, new SqlParameter("@Id", id));
        }

        public override Usuario ObterPorId(int id)
        {
            string sql = "SELECT * FROM Usuarios WHERE Id = @Id";
            var usuarios = db.ExecutarQuery(sql, MapearUsuario, new SqlParameter("@Id", id));
            return usuarios.FirstOrDefault();
        }

        public override List<Usuario> ObterTodos()
        {
            string sql = "SELECT * FROM Usuarios ORDER BY Nome";
            return db.ExecutarQuery(sql, MapearUsuario);
        }

        public Usuario ObterPorUsername(string username)
        {
            string sql = "SELECT * FROM Usuarios WHERE Username = @Username";
            var usuarios = db.ExecutarQuery(sql, MapearUsuario, new SqlParameter("@Username", username));
            return usuarios.FirstOrDefault();
        }

        public List<Usuario> ObterComMultas()
        {
            string sql = "SELECT * FROM Usuarios WHERE MultaTotal > 0 ORDER BY MultaTotal DESC";
            return db.ExecutarQuery(sql, MapearUsuario);
        }

        private Usuario MapearUsuario(SqlDataReader reader)
        {
            var usuario = new Usuario(
                reader["Nome"].ToString(),
                reader["Email"].ToString(),
                reader["Telefone"]?.ToString(),
                reader["Username"].ToString(),
                "", // Password não é necessária na leitura
                (TipoUsuario)(int)reader["Tipo"],
                (int)reader["Idade"]
            );

            usuario.Id = (int)reader["Id"];
            usuario.PasswordHash = reader["PasswordHash"].ToString();
            usuario.Ativo = (bool)reader["Ativo"];
            usuario.MultaTotal = (decimal)reader["MultaTotal"];
            usuario.DataCriacao = (DateTime)reader["DataCriacao"];

            return usuario;
        }
    }

    // Repositório de Livros
    public class RepositorioLivros : RepositorioBase<Livro>
    {
        public RepositorioLivros(DatabaseManager database) : base(database) { }

        public override void Inserir(Livro livro)
        {
            string sql = @"INSERT INTO Livros (Titulo, Autor, ISBN, IdadeMinima, Disponivel, Genero, DataCriacao)
                          OUTPUT INSERTED.Id
                          VALUES (@Titulo, @Autor, @ISBN, @IdadeMinima, @Disponivel, @Genero, @DataCriacao)";

            livro.Id = db.ExecutarScalar<int>(sql,
                new SqlParameter("@Titulo", livro.Titulo),
                new SqlParameter("@Autor", livro.Autor),
                new SqlParameter("@ISBN", livro.ISBN ?? (object)DBNull.Value),
                new SqlParameter("@IdadeMinima", livro.IdadeMinima),
                new SqlParameter("@Disponivel", livro.Disponivel),
                new SqlParameter("@Genero", livro.Genero ?? (object)DBNull.Value),
                new SqlParameter("@DataCriacao", livro.DataCriacao)
            );
        }

        public override void Atualizar(Livro livro)
        {
            string sql = @"UPDATE Livros SET Titulo = @Titulo, Autor = @Autor, ISBN = @ISBN, 
                          IdadeMinima = @IdadeMinima, Disponivel = @Disponivel, Genero = @Genero
                          WHERE Id = @Id";

            db.ExecutarComando(sql,
                new SqlParameter("@Id", livro.Id),
                new SqlParameter("@Titulo", livro.Titulo),
                new SqlParameter("@Autor", livro.Autor),
                new SqlParameter("@ISBN", livro.ISBN ?? (object)DBNull.Value),
                new SqlParameter("@IdadeMinima", livro.IdadeMinima),
                new SqlParameter("@Disponivel", livro.Disponivel),
                new SqlParameter("@Genero", livro.Genero ?? (object)DBNull.Value)
            );
        }

        public override void Eliminar(int id)
        {
            string sql = "DELETE FROM Livros WHERE Id = @Id";
            db.ExecutarComando(sql, new SqlParameter("@Id", id));
        }

        public override Livro ObterPorId(int id)
        {
            string sql = "SELECT * FROM Livros WHERE Id = @Id";
            var livros = db.ExecutarQuery(sql, MapearLivro, new SqlParameter("@Id", id));
            return livros.FirstOrDefault();
        }

        public override List<Livro> ObterTodos()
        {
            string sql = "SELECT * FROM Livros ORDER BY Titulo";
            return db.ExecutarQuery(sql, MapearLivro);
        }

        public List<Livro> ObterDisponiveis()
        {
            string sql = "SELECT * FROM Livros WHERE Disponivel = 1 ORDER BY Titulo";
            return db.ExecutarQuery(sql, MapearLivro);
        }

        public List<Livro> ObterEmprestados()
        {
            string sql = "SELECT * FROM Livros WHERE Disponivel = 0 ORDER BY Titulo";
            return db.ExecutarQuery(sql, MapearLivro);
        }

        private Livro MapearLivro(SqlDataReader reader)
        {
            var livro = new Livro(
                reader["Titulo"].ToString(),
                reader["Autor"].ToString(),
                reader["ISBN"]?.ToString(),
                (int)reader["IdadeMinima"],
                reader["Genero"]?.ToString()
            );

            livro.Id = (int)reader["Id"];
            livro.Disponivel = (bool)reader["Disponivel"];
            livro.DataCriacao = (DateTime)reader["DataCriacao"];

            return livro;
        }
    }

    // Repositório de Empréstimos
    public class RepositorioEmprestimos : RepositorioBase<Emprestimo>
    {
        private readonly RepositorioUsuarios repoUsuarios;
        private readonly RepositorioLivros repoLivros;

        public RepositorioEmprestimos(DatabaseManager database, RepositorioUsuarios repoUsuarios, RepositorioLivros repoLivros)
            : base(database)
        {
            this.repoUsuarios = repoUsuarios;
            this.repoLivros = repoLivros;
        }

        public override void Inserir(Emprestimo emprestimo)
        {
            string sql = @"INSERT INTO Emprestimos (IdUsuario, IdLivro, DataEmprestimo, DataPrevistaDevolucao, Status, MultaAplicada)
                          OUTPUT INSERTED.Id
                          VALUES (@IdUsuario, @IdLivro, @DataEmprestimo, @DataPrevistaDevolucao, @Status, @MultaAplicada)";

            emprestimo.Id = db.ExecutarScalar<int>(sql,
                new SqlParameter("@IdUsuario", emprestimo.IdUsuario),
                new SqlParameter("@IdLivro", emprestimo.IdLivro),
                new SqlParameter("@DataEmprestimo", emprestimo.DataEmprestimo),
                new SqlParameter("@DataPrevistaDevolucao", emprestimo.DataPrevistaDevolucao),
                new SqlParameter("@Status", (int)emprestimo.Status),
                new SqlParameter("@MultaAplicada", emprestimo.MultaAplicada)
            );
        }

        public override void Atualizar(Emprestimo emprestimo)
        {
            string sql = @"UPDATE Emprestimos SET DataDevolucao = @DataDevolucao, Status = @Status, 
                          MultaAplicada = @MultaAplicada WHERE Id = @Id";

            db.ExecutarComando(sql,
                new SqlParameter("@Id", emprestimo.Id),
                new SqlParameter("@DataDevolucao", emprestimo.DataDevolucao ?? (object)DBNull.Value),
                new SqlParameter("@Status", (int)emprestimo.Status),
                new SqlParameter("@MultaAplicada", emprestimo.MultaAplicada)
            );
        }

        public override void Eliminar(int id)
        {
            string sql = "DELETE FROM Emprestimos WHERE Id = @Id";
            db.ExecutarComando(sql, new SqlParameter("@Id", id));
        }

        public override Emprestimo ObterPorId(int id)
        {
            string sql = "SELECT * FROM Emprestimos WHERE Id = @Id";
            var emprestimos = db.ExecutarQuery(sql, MapearEmprestimo, new SqlParameter("@Id", id));
            return emprestimos.FirstOrDefault();
        }

        public override List<Emprestimo> ObterTodos()
        {
            string sql = "SELECT * FROM Emprestimos ORDER BY DataEmprestimo DESC";
            return db.ExecutarQuery(sql, MapearEmprestimo);
        }

        public List<Emprestimo> ObterAtivos()
        {
            string sql = "SELECT * FROM Emprestimos WHERE Status = 1 ORDER BY DataEmprestimo DESC";
            return db.ExecutarQuery(sql, MapearEmprestimo);
        }

        public List<Emprestimo> ObterPorUsuario(int idUsuario)
        {
            string sql = "SELECT * FROM Emprestimos WHERE IdUsuario = @IdUsuario ORDER BY DataEmprestimo DESC";
            return db.ExecutarQuery(sql, MapearEmprestimo, new SqlParameter("@IdUsuario", idUsuario));
        }

        public List<Emprestimo> ObterAtrasados()
        {
            string sql = @"SELECT * FROM Emprestimos 
                          WHERE Status = 1 AND DataPrevistaDevolucao < GETDATE() 
                          ORDER BY DataPrevistaDevolucao";
            return db.ExecutarQuery(sql, MapearEmprestimo);
        }

        private Emprestimo MapearEmprestimo(SqlDataReader reader)
        {
            var emprestimo = new Emprestimo((int)reader["IdUsuario"], (int)reader["IdLivro"]);

            emprestimo.Id = (int)reader["Id"];
            emprestimo.DataEmprestimo = (DateTime)reader["DataEmprestimo"];
            emprestimo.DataPrevistaDevolucao = (DateTime)reader["DataPrevistaDevolucao"];
            emprestimo.DataDevolucao = reader["DataDevolucao"] == DBNull.Value ? null : (DateTime?)reader["DataDevolucao"];
            emprestimo.Status = (StatusEmprestimo)(int)reader["Status"];
            emprestimo.MultaAplicada = (decimal)reader["MultaAplicada"];

            // Carregar dados relacionados
            emprestimo.Usuario = repoUsuarios.ObterPorId(emprestimo.IdUsuario);
            emprestimo.Livro = repoLivros.ObterPorId(emprestimo.IdLivro);

            return emprestimo;
        }
    }

    // Serviços de Negócio
    public class ServicoAutenticacao
    {
        private readonly RepositorioUsuarios repoUsuarios;

        public ServicoAutenticacao(RepositorioUsuarios repositorio)
        {
            repoUsuarios = repositorio;
        }

        public Usuario Login(string username, string password)
        {
            var usuario = repoUsuarios.ObterPorUsername(username);
            if (usuario != null && usuario.Ativo && usuario.VerificarPassword(password))
            {
                return usuario;
            }
            return null;
        }

        public bool RegistrarUsuario(string nome, string email, string telefone, string username, string password, TipoUsuario tipo, int idade)
        {
            try
            {
                var usuarioExistente = repoUsuarios.ObterPorUsername(username);
                if (usuarioExistente != null)
                {
                    return false; // Username já existe
                }

                var novoUsuario = new Usuario(nome, email, telefone, username, password, tipo, idade);
                repoUsuarios.Inserir(novoUsuario);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

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
                    $"Livro devolvido com multa de €{multa:F2} por atraso." :
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
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║     SISTEMA DE GESTÃO BIBLIOTECA     ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

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

       /* private void MostrarEstatisticas()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("           ESTATÍSTICAS GERAIS");
            Console.WriteLine("═══════════════════════════════════════");

            try
            {
                // Estatísticas de livros
                var livros = servicoLivros.ListarTodos();
                var livrosDisponiveis = livros.Count(l => l.Disponivel);
                var livrosEmprestados = livros.Count - livrosDisponiveis;

                Console.WriteLine($"📚 LIVROS:");
                Console.WriteLine($"   • Total de livros: {livros.Count}");
                Console.WriteLine($"   • Disponíveis: {livrosDisponiveis}");
                Console.WriteLine($"   • Emprestados: {livrosEmprestados}");

                // Estatísticas de usuários
                var usuarios = servicoUsuarios.ListarTodos();
                var clientes = usuarios.Count(u => u.TipoUsuario == TipoUsuario.Cliente);
                var funcionarios = usuarios.Count(u => u.TipoUsuario == TipoUsuario.Recepcao);
                var admins = usuarios.Count(u => u.TipoUsuario == TipoUsuario.Admin);

                Console.WriteLine($"\n👥 USUÁRIOS:");
                Console.WriteLine($"   • Total de usuários: {usuarios.Count}");
                Console.WriteLine($"   • Clientes: {clientes}");
                Console.WriteLine($"   • Funcionários: {funcionarios}");
                Console.WriteLine($"   • Administradores: {admins}");

                // Estatísticas de empréstimos
                var emprestimos = servicoEmprestimos.ListarTodos();
                var emprestimosAtivos = emprestimos.Count(e => e.Status == StatusEmprestimo.Ativo);
                var emprestimosAtrasados = emprestimos.Count(e => e.Status == StatusEmprestimo.Atrasado);
                var emprestimosDevolvidos = emprestimos.Count(e => e.Status == StatusEmprestimo.Devolvido);

                Console.WriteLine($"\n📋 EMPRÉSTIMOS:");
                Console.WriteLine($"   • Total de empréstimos: {emprestimos.Count}");
                Console.WriteLine($"   • Ativos: {emprestimosAtivos}");
                Console.WriteLine($"   • Atrasados: {emprestimosAtrasados}");
                Console.WriteLine($"   • Devolvidos: {emprestimosDevolvidos}");

                // Top 5 livros mais emprestados
                var livrosEmprestadosCount = emprestimos
                    .GroupBy(e => e.LivroId)
                    .Select(g => new { LivroId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                Console.WriteLine($"\n📊 TOP 5 LIVROS MAIS EMPRESTADOS:");
                if (livrosEmprestadosCount.Any())
                {
                    foreach (var item in livrosEmprestadosCount)
                    {
                        var livro = livros.FirstOrDefault(l => l.Id == item.LivroId);
                        if (livro != null)
                        {
                            Console.WriteLine($"   • {livro.Titulo} - {item.Count} empréstimos");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("   • Nenhum empréstimo registado");
                }

                // Multas pendentes
                var multasPendentes = emprestimos
                    .Where(e => e.Status == StatusEmprestimo.Atrasado ||
                               (e.Status == StatusEmprestimo.Devolvido && e.DataDevolucao > e.DataPrevistaDevolucao))
                    .Sum(e => e.CalcularMulta());

                Console.WriteLine($"\n💰 MULTAS:");
                Console.WriteLine($"   • Total de multas pendentes: {multasPendentes:C}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }*/
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
        }
        

        private void RegistrarUsuario()
        {
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
        }

        private void MostrarMenuPrincipal()
        {
            Console.WriteLine($"\n═══ MENU PRINCIPAL - {usuarioLogado.Nome} ({usuarioLogado.Tipo}) ═══");

            switch (usuarioLogado.Tipo)
            {
                case TipoUsuario.Cliente:
                    MenuCliente();
                    break;
                case TipoUsuario.Recepcao:
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
        }

        private void EditarLivro()
        {
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
        }

        private void RemoverLivro()
        {
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
            }
        }

        // Métodos de Gestão de Usuários
        private void MenuGestaoUsuarios()
        {
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
        }

        private void ToggleUsuarioAtivo()
        {
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
        }

        // Métodos de Empréstimos
        private void CriarEmprestimo()
        {
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
        }

        private void DevolverLivro()
        {
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
                    Console.WriteLine($"Multa aplicada: €{resultado.multa:F2}");
                }
            }
            else
            {
                Console.WriteLine($"\n✗ {resultado.mensagem}");
            }
        }

        // Métodos de Listagem
        private void ListarTodosLivros()
        {
            var livros = repoLivros.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS LIVROS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
        }

        private void ListarLivrosDisponiveis()
        {
            var livros = repoLivros.ObterDisponiveis();
            Console.WriteLine("\n═══ LIVROS DISPONÍVEIS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
        }

        private void MenuListarLivros()
        {
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
            var livros = repoLivros.ObterEmprestados();
            Console.WriteLine("\n═══ LIVROS EMPRESTADOS ═══");
            foreach (var livro in livros)
            {
                livro.ExibirInformacoes();
            }
        }

        private void ListarTodosUsuarios()
        {
            var usuarios = repoUsuarios.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS USUÁRIOS ═══");
            foreach (var usuario in usuarios)
            {
                usuario.ExibirInformacoes();
            }
        }

        private void ListarUsuariosComMultas()
        {
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
        }

        private void ListarEmprestimosAtivos()
        {
            var emprestimos = repoEmprestimos.ObterAtivos();
            Console.WriteLine("\n═══ EMPRÉSTIMOS ATIVOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
        }

        private void ListarEmprestimosAtrasados()
        {
            var emprestimos = repoEmprestimos.ObterAtrasados();
            Console.WriteLine("\n═══ EMPRÉSTIMOS ATRASADOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
        }

        private void ListarEmprestimosUsuario(int idUsuario)
        {
            var emprestimos = repoEmprestimos.ObterPorUsuario(idUsuario);
            Console.WriteLine("\n═══ MEUS EMPRÉSTIMOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
        }

        // Métodos auxiliares
        private void PesquisarLivros()
        {
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
        }

        private void PesquisarUsuario()
        {
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
        }

        private void MostrarMultasUsuario()
        {
            var usuario = repoUsuarios.ObterPorId(usuarioLogado.Id);
            Console.WriteLine($"\n═══ MINHAS MULTAS ═══");
            Console.WriteLine($"Total de multas: €{usuario.MultaTotal:F2}");

            var emprestimos = repoEmprestimos.ObterPorUsuario(usuarioLogado.Id);
            var comMulta = emprestimos.Where(e => e.MultaAplicada > 0).ToList();

            if (comMulta.Any())
            {
                Console.WriteLine("\nDetalhes das multas:");
                foreach (var emp in comMulta)
                {
                    Console.WriteLine($"- {emp.Livro.Titulo}: €{emp.MultaAplicada:F2}");
                }
            }
        }

        private void MenuGestaoEmprestimos()
        {
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
            var emprestimos = repoEmprestimos.ObterTodos();
            Console.WriteLine("\n═══ TODOS OS EMPRÉSTIMOS ═══");
            foreach (var emprestimo in emprestimos)
            {
                emprestimo.ExibirInformacoes();
                Console.WriteLine();
            }
        }

        private void ListarHistoricoEmprestimos()
        {
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
        }

        private void MenuRelatorios()
        {
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
        }

        private void RelatorioUsuariosMaisAtivos()
        {
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
        }

        private void RelatorioMultas()
        {
            var usuarios = repoUsuarios.ObterComMultas();
            decimal totalMultas = usuarios.Sum(u => u.MultaTotal);

            Console.WriteLine("\n═══ RELATÓRIO DE MULTAS ═══");
            Console.WriteLine($"Total arrecadado em multas: €{totalMultas:F2}");
            Console.WriteLine($"Usuários com multas: {usuarios.Count}");
            Console.WriteLine("\nDetalhes:");
            foreach (var usuario in usuarios.Take(10))
            {
                Console.WriteLine($"{usuario.Nome}: €{usuario.MultaTotal:F2}");
            }
        }

        private void RelatorioEmprestimosPorPeriodo()
        {
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
        }
        static void Main(string[] args)
        {
            // Define a connection string para a base de dados SQL Server
            string connectionString = @"Server=localhost;Database=BibliotecaDB;Trusted_Connection=True;";

            // Cria a instância principal do sistema
            var sistema = new SistemaBiblioteca(connectionString);

            // Inicia o sistema
            sistema.Iniciar();
        }
    }
}