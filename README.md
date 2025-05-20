# 🏍️ UWBike API  

API RESTful desenvolvida para o gerenciamento de motos em um sistema de frota utilizando ASP.NET Core e Oracle Database.  

---

## 📌 **Descrição do Projeto**
UWBike é uma aplicação backend que permite o cadastro, atualização, consulta e remoção de motos em um banco de dados Oracle. A aplicação foi desenvolvida em ASP.NET Core utilizando Entity Framework Core para integração com o banco de dados.

---

## 🚀 **Tecnologias Utilizadas**
- **ASP.NET Core 7.0**
- **Entity Framework Core**
- **Oracle Database**
- **Swagger UI**
- **Oracle SQL Developer**

---
## 📂 **Estrutura de Pastas**
- **Connection/** → Contém a configuração do banco de dados e o contexto do Entity Framework Core.
  - `AppDbContext.cs` → Classe responsável pela conexão e configuração do EF Core.

- **Controllers/** → Controladores da aplicação, responsáveis por definir as rotas da API.
  - `MotoController.cs` → Controller principal para CRUD das motos.

- **Data/Mappings/** → Configurações de mapeamento das entidades para o banco de dados.
  - `MotoMapping.cs` → Mapeamento da entidade `Moto` para a tabela no Oracle.

- **Migrations/** → Arquivos de migração gerados pelo Entity Framework para criar e atualizar o banco.
  - `20250520010129_MotoMappingMigration.cs` → Migração que cria a tabela de motos.
  - `AppDbContextModelSnapshot.cs` → Representação atual do banco para controle de mudanças.

- **Model/** → Modelos das entidades do sistema.
  - `Moto.cs` → Classe que representa a entidade `Moto` no banco.

- **Properties/** → Configurações de ambiente da aplicação.
  - `launchSettings.json` → Define portas e ambiente de execução.

- **Raiz do Projeto**
  - `appsettings.json` → Configurações da string de conexão com o banco de dados Oracle.
  - `Program.cs` → Arquivo principal para configuração dos serviços e execução do servidor.
  - `UWBike.http` → Arquivo para testes HTTP das rotas.
  - `README.md` → Documentação do projeto.


---

## 🔗 **Rotas da API**
| Método | Rota             | Descrição                             |
|---------|------------------|---------------------------------------|
| `GET`   | `/motos`        | Lista todas as motos cadastradas      |
| `GET`   | `/motos/{id}`   | Consulta uma moto pelo ID             |
| `GET`   | `/motos/buscar` | Busca uma moto pela placa             |
| `POST`  | `/motos`        | Adiciona uma nova moto                |
| `PUT`   | `/motos/{id}`   | Atualiza os dados de uma moto         |
| `DELETE`| `/motos/{id}`   | Remove uma moto do banco de dados     |

---

## ⚙️ **Configuração do Banco de Dados**
1. No arquivo `appsettings.json`, configure a string de conexão para o Oracle:
    ```json
    "ConnectionStrings": {
        "OracleConnection": "User Id= ;Password= ;Data Source=oracle.fiap.com.br:1521/ORCL;"
    }
    ```

2. Realize as migrations:
    ```bash
    dotnet ef migrations add InitialMigration
    dotnet ef database update
    ```

---

## 💻 **Instalação e Execução**
1. Clone o repositório:
    ```bash
    git clone https://github.com/rafaelromanini/UWBike.git
    ```

2. Entre no diretório do projeto:
    ```bash
    cd UWBike
    ```

3. Restaure as dependências:
    ```bash
    dotnet restore
    ```

4. Compile o projeto:
    ```bash
    dotnet build
    ```

5. Execute o projeto:
    ```bash
    dotnet run
    ```

---

## 🔎 **Testes no Swagger**
- Acesse no navegador: [http://localhost:5241/swagger](http://localhost:5241/swagger)  
- Teste todas as rotas diretamente pela interface gráfica.

---

### 🚀 **Próximos Passos:**
- Adicionar testes unitários para as rotas.
- Implementar paginação na listagem de motos.
- Criar integração com um serviço de terceiros para validação de chassis.
