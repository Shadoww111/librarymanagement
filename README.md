
# 📚 Sistema de Gestão de Biblioteca (C# Console + SQL Server)

Este é um projeto de aplicação de consola em C#, com ligação a uma base de dados SQL Server, para gerir uma biblioteca. É baseado em princípios de **Programação Orientada a Objetos (POO)**.

## ✅ Funcionalidades

- 📖 **Gestão de livros**
  - Adicionar livros
  - Listar livros
  - Verificar disponibilidade

- 👤 **Gestão de utilizadores**
  - Adicionar utilizadores
  - Listar utilizadores
  - Tipos: `Cliente`, `Receção`, `Admin`

- 🔁 **Empréstimos**
  - Criar empréstimos
  - Validação da idade mínima e disponibilidade
  - Data de empréstimo guardada

- 📥 **Devoluções**
  - Registo de devolução de livros
  - Multa de 1€ por dia após 5 dias

- 🔎 **Listagens**
  - Livros emprestados com nome do utilizador
  - Utilizadores em incumprimento com o valor da multa

## 🗃️ Estrutura do Projeto

```
📁 BibliotecaConsoleApp/
├── Program.cs               # Interface no terminal (menu)
├── Livro.cs
├── Usuario.cs
├── Emprestimo.cs
├── ConexaoBD.cs
├── LivroDAL.cs
├── UsuarioDAL.cs
└── EmprestimoDAL.cs
└── README.md
```

## 🛠️ Tecnologias Utilizadas

- 💻 Linguagem: **C# (.NET)**
- 💾 Base de Dados: **SQL Server**
- 🧱 Paradigma: **Programação Orientada a Objetos**

## 🔌 Configuração da Base de Dados

Executa o seguinte script no SQL Server Management Studio:

```sql
CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;

CREATE TABLE Livros (
    LivroID INT PRIMARY KEY IDENTITY,
    Titulo NVARCHAR(100),
    Autor NVARCHAR(100),
    FaixaEtaria INT,
    Disponivel BIT
);

CREATE TABLE Usuarios (
    UsuarioID INT PRIMARY KEY IDENTITY,
    Nome NVARCHAR(100),
    Idade INT,
    Tipo NVARCHAR(50)
);

CREATE TABLE Emprestimos (
    EmprestimoID INT PRIMARY KEY IDENTITY,
    LivroID INT FOREIGN KEY REFERENCES Livros(LivroID),
    UsuarioID INT FOREIGN KEY REFERENCES Usuarios(UsuarioID),
    DataEmprestimo DATE,
    DataDevolucao DATE NULL
);
```

## 🚀 Como Executar

1. Abre o projeto no Visual Studio como aplicação de consola (.NET).
2. Garante que tens a base de dados `BibliotecaDB` criada e com as tabelas.
3. Altera a `ConnectionString` se necessário (ficheiro `ConexaoBD.cs`).
4. Executa o projeto e usa o menu no terminal para interagir.

---

