# ⛪ Santuario

<p align="center">
  <strong>Sistema Oficial do Santuário Nossa Senhora da Conceição Aparecida</strong><br/>
  📍 Bela Cruz - CE
</p>

------------------------------------------------------------------------

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/ASP.NET-Core-5C2D91?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/EF%20Core-8.0-6DB33F?style=for-the-badge" />
  <img src="https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/Architecture-Clean-000000?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Status-Em%20Desenvolvimento-28a745?style=for-the-badge" />
</p>

------------------------------------------------------------------------

## 📌 Sobre o Projeto

O **Santuario** é uma aplicação web desenvolvida em **.NET 8**,
utilizando arquitetura em camadas e boas práticas de engenharia de
software.

O sistema foi criado para apoiar a presença digital do Santuário Nossa
Senhora da Conceição Aparecida, permitindo gestão institucional,
notícias e controle administrativo.

------------------------------------------------------------------------

## 🏗️ Arquitetura

    SantuarioCore/
    ├── Santuario.Admin     → Painel Administrativo (Backoffice)
    ├── Santuario.Cliente   → Site Público
    ├── Santuario.Negocio   → Regras + Serviços + DbContext + Migrations
    ├── Santuario.Entidade  → Entidades + ViewModels + Base Auditoria
    └── Directory.Packages.props → Centralização de versões NuGet

------------------------------------------------------------------------

## 📊 Diagrama de Arquitetura

``` mermaid
flowchart TB
  subgraph Presentation["Camada de Apresentação"]
    Admin["Santuario.Admin (MVC Backoffice)"]
    Cliente["Santuario.Cliente (MVC Site Público)"]
  end

  subgraph Business["Camada de Negócio"]
    Negocio["Santuario.Negocio (Regras + DbContext + Migrations)"]
    Security["SenhaHelper (PBKDF2 + Salt)"]
    Seed["Seed Administrador Inicial"]
  end

  subgraph Domain["Camada de Domínio"]
    Entidade["Santuario.Entidade (Entities + Enums + Auditoria)"]
  end

  subgraph Data["Banco de Dados"]
    Db[(PostgreSQL)]
  end

  Admin --> Negocio
  Cliente --> Negocio
  Negocio --> Entidade
  Negocio --> Db
  Negocio --> Security
  Negocio --> Seed
```

------------------------------------------------------------------------

## 🔐 Fluxo de Autenticação

``` mermaid
sequenceDiagram
  autonumber
  actor U as Usuário
  participant C as LoginController
  participant N as LoginNegocio
  participant DB as PostgreSQL
  participant H as SenhaHelper
  participant A as CookieAuth

  U->>C: Envia login + senha
  C->>N: AutenticarAsync()
  N->>DB: Busca usuário por login
  DB-->>N: Retorna hash + salt
  N->>H: VerificarSenha()
  alt Senha válida
    N-->>C: Usuário autenticado
    C->>A: Gera Cookie
    C-->>U: Redirect Home
  else Inválido
    N-->>C: null
    C-->>U: Mensagem erro
  end
```

------------------------------------------------------------------------

## ⚙️ Tecnologias Utilizadas

-   .NET 8
-   ASP.NET Core MVC
-   Entity Framework Core 8
-   PostgreSQL
-   Npgsql
-   PBKDF2 (HMACSHA256)
-   Arquitetura em Camadas

------------------------------------------------------------------------

## ✨ Funcionalidades

-   Autenticação com Hash + Salt (PBKDF2)
-   Seed automático do primeiro administrador
-   Controle de auditoria (DataCriacao / DataAlteracao)
-   Gerenciamento de conteúdo institucional
-   Estrutura preparada para API de doações

------------------------------------------------------------------------

## 🗄️ Banco de Dados

Banco configurado para operar em **UTC**.

A aplicação converte para o fuso horário do Brasil (America/Sao_Paulo)
apenas na exibição.

------------------------------------------------------------------------

## 🚀 Instalação

``` bash
dotnet restore
dotnet build
```

------------------------------------------------------------------------

## 🛠️ Migrations

Criar Migration:

``` bash
dotnet ef migrations add NomeMigration --project Santuario.Negocio --startup-project Santuario.Admin --context SantuarioDbContext
```

Aplicar no banco:

``` bash
dotnet ef database update --project Santuario.Negocio --startup-project Santuario.Admin --context SantuarioDbContext
```

------------------------------------------------------------------------

## 🛣️ Roadmap

-   [ ] CRUD completo de Notícias
-   [ ] Upload de imagens
-   [ ] Comentários autenticados via Google
-   [ ] API de doações online
-   [ ] Dashboard administrativo

------------------------------------------------------------------------

## 🙏 Projeto Institucional

Sistema desenvolvido para apoiar a evangelização digital e organização
administrativa do Santuário Nossa Senhora da Conceição Aparecida -- Bela
Cruz/CE.
