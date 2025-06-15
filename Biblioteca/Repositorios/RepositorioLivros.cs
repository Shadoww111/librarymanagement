using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Modelos;
using Biblioteca.Data;

namespace Biblioteca.Repositorios
{
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
}
