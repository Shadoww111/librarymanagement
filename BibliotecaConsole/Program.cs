using System;
using System.Data.SqlClient;
using System.Collections.Generic;


class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("*** Sistema de Gestão de Biblioteca ***");
            Console.WriteLine("1. Listar livros");
            Console.WriteLine("2. Adicionar livro");
            Console.WriteLine("3. Listar usuários");
            Console.WriteLine("4. Adicionar usuário");
            Console.WriteLine("5. Criar empréstimo");
            Console.WriteLine("6. Devolver livro");
            Console.WriteLine("7. Listar livros emprestados");
            Console.WriteLine("8. Listar usuários com multas");
            Console.WriteLine("9. Sair");
            Console.Write("Escolha uma opção: ");
            switch (Console.ReadLine())
            {
                case "1":
                     break;
                case "2":
                   break;
                case "3":
                     break;
                case "4":
                    break;
                case "5":
                    break;
                case "6":
                     break;
                case "7":
                   break;
                case "8":
                    break;
                case "9": return;
                default: Console.WriteLine("Opção inválida!"); Console.ReadKey(); break;
            }
        }
    }
}