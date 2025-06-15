using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Modelos;
using Biblioteca.Data;
using Biblioteca.Enums;

namespace Biblioteca.Repositorios
{
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
}
