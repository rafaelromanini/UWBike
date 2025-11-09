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

### **Usuários (`/api/v1/usuarios`)**

#### **Criar Usuário:**
```bash
POST /api/v1/usuarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@email.com",
  "senha": "senha123"
}
```

#### **Listar Usuários com Paginação:**
```bash
GET /api/v1/usuarios?pageNumber=1&pageSize=10&search=joão&sortBy=nome&sortDescending=false
```

#### **Buscar Usuário por Email:**
```bash
GET /api/v1/usuarios/buscar?email=joao.silva@email.com
```

#### **Atualizar Usuário:**
```bash
PUT /api/v1/usuarios/1
Content-Type: application/json

{
  "nome": "João Silva Santos",
  "email": "joao.santos@email.com"
}
```

### **Pátios (`/api/v2/patios`)**

#### **Criar Pátio:**
```bash
POST /api/v2/patios
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
GET /api/v2/patios/1/motos?pageNumber=1&pageSize=10
```

### **Motos (`/api/v1/motos`)**

#### **Criar Moto (Regra de Negócio):**
```bash
POST /api/v1/motos
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
GET /api/v1/motos/buscar?placa=ABC-1234
```

#### **Listar Motos com Filtros:**
```bash
GET /api/v1/motos?pageNumber=1&pageSize=5&search=Honda&sortBy=modelo&sortDescending=true
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
      "href": "/api/v1/usuarios/1",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "/api/v1/usuarios/1",
      "rel": "update",
      "method": "PUT"
    },
    {
      "href": "/api/v1/usuarios/1",
      "rel": "delete",
      "method": "DELETE"
    },
    {
      "href": "/api/v1/usuarios",
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

> **⚠️ Versionamento:** Todos os endpoints agora incluem versionamento na URL: `/api/v1/...` ou `/api/v2/...`

### **Autenticação (Público):**
| Método | Endpoint | Versão | Descrição |
|---------|----------|---------|-----------|
| `POST` | `/api/v1/autenticacao/registro` | v1 | Registra um novo usuário |
| `POST` | `/api/v1/autenticacao/login` | v1 | Autentica usuário e retorna token JWT |

### **Usuários (Requer Autenticação):**
| Método | Endpoint | Versão | Descrição |
|---------|----------|---------|-----------|
| `GET` | `/api/v1/usuarios` | v1 | Lista usuários com paginação |
| `GET` | `/api/v1/usuarios/{id}` | v1 | Busca usuário por ID |
| `GET` | `/api/v1/usuarios/buscar?email=` | v1 | Busca por email |
| `PUT` | `/api/v1/usuarios/{id}` | v1 | Atualiza usuário |
| `DELETE` | `/api/v1/usuarios/{id}` | v1 | Remove usuário |

### **Pátios (Requer Autenticação):**
| Método | Endpoint | Versão | Descrição |
|---------|----------|---------|-----------|
| `GET` | `/api/v1/patios` | v1 | Lista pátios com paginação |
| `GET` | `/api/v1/patios/{id}` | v1 | Busca pátio por ID (apenas numérico) |
| `GET` | `/api/v2/patios/{identificador}` | v2 | Busca por ID ou Nome (retorna lista) |
| `GET` | `/api/v1/patios/{id}/motos` | v1 | Lista motos do pátio |
| `POST` | `/api/v1/patios` | v1 | Cria novo pátio |
| `PUT` | `/api/v1/patios/{id}` | v1 | Atualiza pátio |
| `DELETE` | `/api/v1/patios/{id}` | v1 | Remove pátio |

**Exemplos:**
```bash
# v1 - Busca apenas por ID
GET /api/v1/patios/1

# v2 - Busca por ID
GET /api/v2/patios/1

# v2 - Busca por nome (pode retornar múltiplos)
GET /api/v2/patios/Central
```

### **Motos (Requer Autenticação):**
| Método | Endpoint | Versão | Descrição |
|---------|----------|---------|-----------|
| `GET` | `/api/v1/motos` | v1 | Lista motos com paginação |
| `GET` | `/api/v1/motos/{id}` | v1 | Busca moto por ID |
| `GET` | `/api/v1/motos/buscar?placa=` | v1 | Busca por placa |
| `POST` | `/api/v1/motos` | v1 | Cria nova moto (com regra de negócio) |
| `PUT` | `/api/v1/motos/{id}` | v1 | Atualiza moto |
| `DELETE` | `/api/v1/motos/{id}` | v1 | Remove moto |

### **Health Checks (Público):**
| Método | Endpoint | Descrição |
|---------|----------|-----------|
| `GET` | `/health` | Verifica saúde da aplicação com detalhes JSON |

#### **Exemplo de Resposta - `/health`:**
```json
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

## � **Versionamento da API**

A API implementa versionamento através da URL, permitindo evolução sem quebrar clientes existentes.

### **Configuração:**
- **Versão Padrão:** v1.0
- **Formato da URL:** `/api/v{version}/[controller]`
- **Header de Versão:** Retornado em todas as respostas

### **Versões Disponíveis:**

#### **v1.0 (Atual):**
- ✅ Todos os endpoints básicos
- ✅ Autenticação JWT
- ✅ CRUD completo de Usuários, Motos e Pátios
- ✅ Busca de pátios **apenas por ID numérico**

#### **v2.0 (Nova):**
- ✅ Busca de pátios **por ID ou Nome**
- ✅ Retorna **lista de pátios** (suporta múltiplos resultados)
- ✅ Endpoint: `GET /api/v2/patios/{identificador}`

### **Exemplos de Uso:**

**v1 - Busca tradicional por ID:**
```bash
GET /api/v1/patios/1
Authorization: Bearer TOKEN
```
**Resposta v1:**
```json
{
  "data": {
    "id": 1,
    "nome": "Pátio Central",
    ...
  },
  "success": true
}
```

**v2 - Busca por ID ou Nome:**
```bash
# Por ID
GET /api/v2/patios/1
Authorization: Bearer TOKEN

# Por Nome (pode retornar múltiplos)
GET /api/v2/patios/Central
Authorization: Bearer TOKEN
```
**Resposta v2:**
```json
{
  "data": [
    {
      "id": 1,
      "nome": "Pátio Central SP",
      ...
    },
    {
      "id": 2,
      "nome": "Pátio Central RJ",
      ...
    }
  ],
  "success": true,
  "message": "2 pátios encontrados com sucesso"
}
```

### **Comportamento do Versionamento:**
- 🔹 Clientes podem especificar a versão na URL
- 🔹 Se nenhuma versão for especificada, usa v1.0 (padrão)
- 🔹 Header `api-supported-versions` informa versões disponíveis
- 🔹 Versões antigas continuam funcionando (backward compatibility)

---

## �🔐 **Autenticação JWT**

A API utiliza autenticação JWT (JSON Web Token) para proteger os endpoints. Apenas os endpoints de autenticação e health checks são públicos.

### **Como Autenticar:**

1. **Registrar um novo usuário:**
```bash
POST /api/v1/autenticacao/registro
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@email.com",
  "senha": "senha123",
  "confirmacaoSenha": "senha123"
}
```

**Resposta:**
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-11-09T19:30:00Z",
    "email": "joao.silva@email.com",
    "nome": "João Silva"
  },
  "success": true,
  "message": "Usuário registrado com sucesso"
}
```

2. **Fazer login:**
```bash
POST /api/v1/autenticacao/login
Content-Type: application/json

{
  "email": "joao.silva@email.com",
  "senha": "senha123"
}
```

3. **Usar o token nas requisições:**
```bash
GET /api/v1/usuarios
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### **Configurações JWT:**
- **Expiração do Token:** 120 minutos (configurável em `appsettings.json`)
- **Algoritmo:** HMAC SHA-256
- **Senhas:** Criptografadas com BCrypt

### **Segurança Implementada:**
✅ Senhas hasheadas com BCrypt  
✅ Tokens JWT assinados  
✅ Validação de issuer, audience e lifetime  
✅ Proteção de todos os endpoints (exceto autenticação e health checks)  
✅ Integração com Swagger UI para testes autenticados

---

## 🚀 **Tecnologias Utilizadas**
- **ASP.NET Core 9.0** - Framework web moderno e performático
- **Entity Framework Core** - ORM com suporte nativo ao Oracle
- **Oracle Database** - Banco de dados empresarial robusto
- **Swagger/OpenAPI** - Documentação automática e interativa
- **JWT Bearer Authentication** - Autenticação segura com tokens
- **BCrypt.Net** - Criptografia de senhas
- **API Versioning** - Versionamento de endpoints via URL
- **HATEOAS** - Hypermedia as the Engine of Application State
- **Data Annotations** - Validações de modelo integradas
- **Health Checks** - Monitoramento de saúde da aplicação

---

## � **Documentação Adicional**
- **Swagger UI:** Acesse http://localhost:5241 quando a aplicação estiver rodando
- **Oracle SQL Developer:** Para visualização e manutenção do banco
