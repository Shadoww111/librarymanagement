using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Data
{
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
}
