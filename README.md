# 🏍️ UWBike API  

API RESTful desenvolvida para o gerenciamento de motos, usuários e pátios utilizando ASP.NET Core com Oracle Database. Implementa boas práticas REST, paginação, HATEOAS e documentação Swagger completa.

---

# 👥 **Integrantes**
- **Vinicius Leandro de Araujo Bernardes** - RM554728 - TURMA 2TDSPY
- **Edvan Davi Murilo Santos do Nascimento** - RM554733 - TURMA 2TDSPZ  
- **Rafael Romanini de Oliveira** - RM554637 - TURMA 2TDSPZ

---

## 🏗️ **Justificativa da Arquitetura**

### **Domínio Escolhido: Sistema de Gerenciamento de Frota de Motos**
A escolha do domínio de gerenciamento de frota de motos se justifica pela complexidade adequada para demonstrar relacionamentos entre entidades e regras de negócio específicas:

#### **Entidades Principais:**
1. **Usuário** - Representa os operadores do sistema
2. **Pátio** - Locais físicos onde as motos ficam estacionadas
3. **Moto** - Veículos da frota com relacionamento obrigatório com pátios

#### **Arquitetura Técnica:**
- **ASP.NET Core Web API** - Framework robusto com alta performance
- **Entity Framework Core** - ORM maduro com suporte completo ao Oracle
- **Oracle Database** - Banco empresarial com alta confiabilidade
- **Padrão Repository implícito** via DbContext
- **DTOs** para separação de responsabilidades
- **Swagger/OpenAPI** para documentação automática

#### **Justificativas das Escolhas:**
- **Separação de responsabilidades** entre Controllers, Models e Data Access
- **Paginação nativa** para performance em grandes volumes
- **HATEOAS** para navegabilidade da API
- **Validações robustas** com Data Annotations
- **Tratamento de erros** padronizado com status codes apropriados

### **Regra de Negócio Implementada:**
Uma moto **SEMPRE** deve ter um pátio associado. Se uma moto já existir no sistema (mesma placa/chassi) mas não possuir pátio, ela será automaticamente alocada ao pátio especificado no novo cadastro.

---

## 🚀 **Instruções de Execução**

### **Pré-requisitos:**
- .NET 9.0 SDK
- Oracle Database (ou acesso ao oracle.fiap.com.br)
- Visual Studio Code ou Visual Studio

### **1. Clone o Repositório:**
```bash
git clone https://github.com/rafaelromanini/UWBike.git
cd UWBike
```

### **2. Configure a String de Conexão:**
No arquivo `UWBike/appsettings.json`, configure:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=RM554637;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"
  }
}
```

### **3. Instale as Dependências:**
```bash
cd UWBike
dotnet restore
```

### **4. Execute as Migrations:**
```bash
dotnet ef database update
```

### **5. Compile e Execute:**
```bash
dotnet build
dotnet run
```

### **6. Acesse a API:**
- **Swagger UI:** http://localhost:5241
- **API Base:** http://localhost:5241/api

---
## � **Exemplos de Uso dos Endpoints**

### **Usuários (`/api/usuarios`)**

#### **Criar Usuário:**
```bash
POST /api/usuarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@email.com",
  "senha": "senha123"
}
```

#### **Listar Usuários com Paginação:**
```bash
GET /api/usuarios?pageNumber=1&pageSize=10&search=joão&sortBy=nome&sortDescending=false
```

#### **Buscar Usuário por Email:**
```bash
GET /api/usuarios/buscar?email=joao.silva@email.com
```

#### **Atualizar Usuário:**
```bash
PUT /api/usuarios/1
Content-Type: application/json

{
  "nome": "João Silva Santos",
  "email": "joao.santos@email.com"
}
```

### **Pátios (`/api/patios`)**

#### **Criar Pátio:**
```bash
POST /api/patios
Content-Type: application/json

{
  "nome": "Pátio Central",
  "endereco": "Rua das Flores, 123",
  "capacidade": 100,
  "cep": "01234-567",
  "cidade": "São Paulo",
  "estado": "SP",
  "telefone": "11999999999"
}
```

#### **Listar Motos de um Pátio:**
```bash
GET /api/patios/1/motos?pageNumber=1&pageSize=10
```

### **Motos (`/api/motos`)**

#### **Criar Moto (Regra de Negócio):**
```bash
POST /api/motos
Content-Type: application/json

{
  "modelo": "Honda CB 600F Hornet",
  "placa": "ABC-1234",
  "chassi": "9C2JB1310CR000001",
  "patioId": 1,
  "anoFabricacao": 2022,
  "cor": "Vermelha"
}
```

#### **Buscar Moto por Placa:**
```bash
GET /api/motos/buscar?placa=ABC-1234
```

#### **Listar Motos com Filtros:**
```bash
GET /api/motos?pageNumber=1&pageSize=5&search=Honda&sortBy=modelo&sortDescending=true
```

### **Exemplo de Resposta com HATEOAS:**
```json
{
  "data": {
    "id": 1,
    "nome": "João Silva",
    "email": "joao.silva@email.com",
    "dataCriacao": "2025-09-21T10:30:00Z"
  },
  "success": true,
  "message": "Usuário encontrado com sucesso",
  "errors": [],
  "links": [
    {
      "href": "/api/usuarios/1",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "/api/usuarios/1",
      "rel": "update",
      "method": "PUT"
    },
    {
      "href": "/api/usuarios/1",
      "rel": "delete",
      "method": "DELETE"
    },
    {
      "href": "/api/usuarios",
      "rel": "list",
      "method": "GET"
    }
  ]
}
```

---

## 🏗️ **Estrutura do Projeto**

```
UWBike/
├── UWBike/                     # Projeto principal da API
│   ├── Common/                 # Classes utilitárias (HATEOAS, Paginação)
│   ├── Connection/             # Contexto do Entity Framework
│   ├── Controllers/            # Controllers da API REST
│   ├── Data/Mappings/          # Configurações EF Core
│   ├── Migrations/             # Migrations do banco
│   ├── Model/                  # Entidades do domínio
│   ├── Properties/             # Configurações do projeto
│   ├── appsettings.json        # Configurações da aplicação
│   └── Program.cs              # Ponto de entrada da aplicação
├── UWBike.Tests/              # Projeto de testes (para implementação)
└── README.md                  # Documentação do projeto
```

---

## 🔗 **Endpoints da API**

### **Usuários:**
| Método | Endpoint | Descrição |
|---------|----------|-----------|
| `GET` | `/api/usuarios` | Lista usuários com paginação |
| `GET` | `/api/usuarios/{id}` | Busca usuário por ID |
| `GET` | `/api/usuarios/buscar?email=` | Busca por email |
| `POST` | `/api/usuarios` | Cria novo usuário |
| `PUT` | `/api/usuarios/{id}` | Atualiza usuário |
| `DELETE` | `/api/usuarios/{id}` | Remove usuário |

### **Pátios:**
| Método | Endpoint | Descrição |
|---------|----------|-----------|
| `GET` | `/api/patios` | Lista pátios com paginação |
| `GET` | `/api/patios/{id}` | Busca pátio por ID |
| `GET` | `/api/patios/{id}/motos` | Lista motos do pátio |
| `POST` | `/api/patios` | Cria novo pátio |
| `PUT` | `/api/patios/{id}` | Atualiza pátio |
| `DELETE` | `/api/patios/{id}` | Remove pátio |

### **Motos:**
| Método | Endpoint | Descrição |
|---------|----------|-----------|
| `GET` | `/api/motos` | Lista motos com paginação |
| `GET` | `/api/motos/{id}` | Busca moto por ID |
| `GET` | `/api/motos/buscar?placa=` | Busca por placa |
| `POST` | `/api/motos` | Cria nova moto (com regra de negócio) |
| `PUT` | `/api/motos/{id}` | Atualiza moto |
| `DELETE` | `/api/motos/{id}` | Remove moto |

### **Health Checks:**
| Método | Endpoint | Descrição |
|---------|----------|-----------|
| `GET` | `/health` | Verifica saúde da aplicação |

#### **Exemplo de Resposta - `/health`:**
```
{
  "status": "Healthy",
  "timestamp": "2025-11-09T18:30:45.1234567Z",
  "duration": "00:00:00.0123456",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0098765",
      "exception": null,
      "data": {}
    }
  ]
}
```

**Status possíveis:**
- `Healthy` - Todos os checks estão funcionando
- `Degraded` - Sistema funcionando com limitações
- `Unhealthy` - Sistema com problemas críticos

---

## 🚀 **Tecnologias Utilizadas**
- **ASP.NET Core 9.0** - Framework web moderno e performático
- **Entity Framework Core** - ORM com suporte nativo ao Oracle
- **Oracle Database** - Banco de dados empresarial robusto
- **Swagger/OpenAPI** - Documentação automática e interativa
- **HATEOAS** - Hypermedia as the Engine of Application State
- **Data Annotations** - Validações de modelo integradas

---

## � **Documentação Adicional**
- **Swagger UI:** Acesse http://localhost:5241 quando a aplicação estiver rodando
- **Oracle SQL Developer:** Para visualização e manutenção do banco
