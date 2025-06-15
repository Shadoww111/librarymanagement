using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblioteca.Data;

namespace Biblioteca.Repositorios
{
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
}
