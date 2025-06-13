
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

CREATE TABLE Usuarios (
    UsuarioID INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Idade INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL, -- Cliente, Rececao, Admin
    Password NVARCHAR(100) NOT NULL
);

CREATE TABLE Livros (
    LivroID INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(100) NOT NULL,
    Autor NVARCHAR(100) NOT NULL,
    FaixaEtaria INT NOT NULL,
    Disponivel BIT NOT NULL DEFAULT 1
);

CREATE TABLE Emprestimos (
    EmprestimoID INT PRIMARY KEY IDENTITY(1,1),
    LivroID INT NOT NULL,
    UsuarioID INT NOT NULL,
    DataEmprestimo DATE NOT NULL,
    DataDevolucao DATE NULL,

    CONSTRAINT FK_Emprestimos_Livros FOREIGN KEY (LivroID) REFERENCES Livros(LivroID),
    CONSTRAINT FK_Emprestimos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);
```

## 🚀 Como Executar

1. Abre o projeto no Visual Studio como aplicação de consola (.NET).
2. Garante que tens a base de dados `BibliotecaDB` criada e com as tabelas.
3. Altera a `ConnectionString` se necessário (ficheiro `ConexaoBD.cs`).
4. Executa o projeto e usa o menu no terminal para interagir.

---

