
# 📚 Sistema de Gestão de Biblioteca

Este é um sistema de gestão de biblioteca desenvolvido em **C#** em **Console** e acesso a **SQL Server**, utilizando **Programação Orientada a Objetos (POO)**. O sistema permite gerir livros, utilizadores e empréstimos, com diferentes perfis de utilizador (Cliente, Receção, Administrador).

---

## ✨ Funcionalidades

- 🔐 Autenticação de utilizadores
- 👥 Diferentes níveis de acesso
- 📖 Gestão de livros (CRUD)
- 🔄 Empréstimos com controlo de devoluções e multas
- 📈 Relatórios e estatísticas
- 🕒 Atualização automática de empréstimos atrasados
- 🧠 Repositórios separados por responsabilidade

---

## 🗂️ Estrutura do Projeto

```
/SistemaGestaBiblioteca
│
├── Enums/                # Enumerações (TipoUsuario, StatusEmprestimo)
├── Modelos/              # Classes principais (Usuario, Livro, Emprestimo)
├── Repositorios/         # Acesso à base de dados (Repositorios genéricos)
├── Servicos/             # Lógica de negócio (autenticação, empréstimos)
├── Data/                 # DatabaseManager: criação e ligação à DB
├── Sistema/              # Sistema principal (menus e fluxo)
└── Program.cs            # Ponto de entrada
```

---

## ⚙️ Tecnologias Utilizadas

- 💻 **Linguagem:** C# (.NET 6+)
- 🛢 **Base de Dados:** SQL Server
- 🔗 **Acesso a dados:** `SqlConnection`, `SqlCommand`
- 🧱 **Paradigma:** Programação Orientada a Objetos

---

## 🚀 Como Executar

1. **Clonar o projeto**

```bash
git clone https://github.com/o-seu-usuario/sistema-biblioteca.git
cd sistema-biblioteca
```

2. **Atualizar a connection string no `Program.cs`**

```csharp
string connectionString = @"Server=localhost;Database=BibliotecaDB;Trusted_Connection=True;";
```

3. **Executar o projeto**

```bash
dotnet run
```

> O sistema cria automaticamente a base de dados e as tabelas, se ainda não existirem.

---

## 👥 Tipos de Utilizador

| Tipo     | Descrição                                                    |
|----------|--------------------------------------------------------------|
| Cliente  | Pode consultar livros e os seus próprios empréstimos/multas |
| Receção  | Pode registar/devolver empréstimos, gerir livros e utilizadores |
| Admin    | Acesso total à gestão do sistema e relatórios                |

---

## 📝 Conta Admin Padrão

Na primeira execução, será criado automaticamente:

```
Username: admin
Password: admin123
```

---

## 📚 Exemplo de Dados para Testes

### Inserção de Livros

```sql
INSERT INTO Livros (Titulo, Autor, ISBN, IdadeMinima, Disponivel, Genero)
VALUES
('1984', 'George Orwell', '978-0451524935', 16, 1, 'Distopia'),
('Orgulho e Preconceito', 'Jane Austen', '978-0141439518', 14, 1, 'Romance'),
('O Código Da Vinci', 'Dan Brown', '978-0307474278', 16, 1, 'Mistério'),
('O Pequeno Príncipe', 'Antoine de Saint-Exupéry', '978-2070612758', 8, 1, 'Infantil'),
('A Revolta de Atlas', 'Ayn Rand', '978-0451191147', 18, 1, 'Filosofia'),
('Cem Anos de Solidão', 'Gabriel García Márquez', '978-0060883287', 16, 1, 'Realismo Mágico'),
('A Menina que Roubava Livros', 'Markus Zusak', '978-0375842207', 12, 1, 'Drama'),
('A Guerra dos Tronos', 'George R. R. Martin', '978-0553593716', 18, 1, 'Fantasia Épica'),
('O Alquimista', 'Paulo Coelho', '978-0061122415', 14, 1, 'Ficção Espiritual'),
('O Hobbit', 'J.R.R. Tolkien', '978-0547928227', 10, 1, 'Fantasia'),
('Moby Dick', 'Herman Melville', '978-1503280786', 14, 1, 'Aventura'),
('As Aventuras de Sherlock Holmes', 'Arthur Conan Doyle', '978-1514699353', 12, 1, 'Mistério'),
('O Diário de Anne Frank', 'Anne Frank', '978-0553296983', 13, 1, 'História'),
('Percy Jackson e o Ladrão de Raios', 'Rick Riordan', '978-1423134947', 10, 1, 'Fantasia Juvenil');
```

### Inserção de Utilizadores

```sql
INSERT INTO Usuarios (Nome, Email, Telefone, Username, PasswordHash, Tipo, Idade, Ativo, MultaTotal)
VALUES
('Ana Costa', 'ana.costa@email.com', '919876543', 'anacosta', 'hash_ana_123', 1, 22, 1, 0),
('Carlos Mendes', 'carlos.mendes@email.com', '912112233', 'cmendes', 'hash_carlos_456', 2, 35, 1, 3.75),
('Rita Lopes', 'rita.lopes@email.com', '917654321', 'ritalopes', 'hash_rita_789', 1, 28, 1, 0),
('Tiago Ferreira', 'tiago.ferreira@email.com', NULL, 'tferreira', 'hash_tiago_000', 2, 45, 1, 12.50),
('Sofia Almeida', 'sofia.almeida@email.com', '914789123', 'sofiaa', 'hash_sofia_xyz', 1, 19, 1, 0),
('Miguel Rocha', 'miguel.rocha@email.com', '915555444', 'miguelrocha', 'hash_miguel_pwd', 3, 33, 1, 0),
('Beatriz Nunes', 'beatriz.nunes@email.com', NULL, 'beatrizn', 'hash_beatriz_a1b2', 1, 26, 1, 1.25),
('Joana Marques', 'joana.marques@email.com', '918888111', 'joanamarques', 'hash_joana_pass', 2, 31, 1, 0),
('André Pinto', 'andre.pinto@email.com', '916666000', 'andrep', 'hash_andre_secure', 1, 24, 0, 0),
('Laura Ramos', 'laura.ramos@email.com', '911234567', 'laurar', 'hash_laura_xyz', 1, 27, 1, 6.80);
```

> ⚠️ As passwords estão em formato hash fictício. No sistema real, estas devem ser geradas via `SHA256`.

---

## 🧾 Licença

Este projeto é aberto para fins educativos. Sinta-se livre para modificar, melhorar e personalizar.
