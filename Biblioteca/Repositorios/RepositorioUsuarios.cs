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
}
