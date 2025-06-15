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
}
