using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Biblioteca.Sistema;

namespace SistemaGestaBiblioteca
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Server=localhost;Database=BibliotecaDB;Trusted_Connection=True;";
            var sistema = new SistemaBiblioteca(connectionString);
            sistema.Iniciar();
        }
    }
   
}