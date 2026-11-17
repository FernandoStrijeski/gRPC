# 🚀 Enterprise gRPC .NET 9 Architecture

> Um projeto moderno, robusto e de nível corporativo desenvolvido em **.NET 9**, implementando os principais padrões de arquitetura limpa, segurança avançada de APIs, resiliência e boas práticas de engenharia de software para serviços gRPC.

> ### 🌐 O que é o gRPC?
> **gRPC** é um framework de código aberto de alta performance criado pelo Google. Ele utiliza o protocolo **HTTP/2** e o **Protocol Buffers (Protobuf)** — um mecanismo onde os dados são compactados em formato **binário** e estritamente tipados por meio de contratos definidos em arquivos `.proto`. 
> 
> Ele serve para conectar sistemas e microsserviços de forma ultrarrápida, utilizando contratos rígidos e máxima economia de rede.

---

### 🏢 Na Prática Corporativa

Em ambientes de alta volumetria — como serviços bancários, plataformas de streaming ou grandes sistemas corporativos de RH — aplicações precisam se comunicar milhares de vezes por segundo:

* **O Problema do JSON:** Trocar dados em texto puro (JSON) gera sobrecarga de processamento e payloads pesados.
* **A Solução gRPC:** Traduz as chamadas para pacotes binários compactos que trafegam com latência mínima, garantindo que cliente e servidor falem exatamente a mesma língua, sem margem para ambiguidade.

---

## 🛠️ Tecnologias e Stack Tecnológica

* **Plataforma:** .NET 9 / C#
* **Protocolo:** gRPC sobre HTTP/2 (com suporte a TLS/HTTPS)
* **Autenticação & Segurança:** JWT (*JSON Web Tokens*) com `System.IdentityModel.Tokens.Jwt` e Interceptors customizados.
* **Validação de Dados:** *FluentValidation* integrado para validações rigorosas de regras de negócio.
* **Resiliência & Tratamento de Erros:** Global `ExceptionInterceptor` com mapeamento para códigos de status gRPC oficiais.
* **Proteção de Rede:** *Rate Limiting* nativo integrado para mitigação de ataques DoS e controle de fluxo.
* **Arquitetura:** Padrão *Slim Program* com extensões de Injeção de Dependência modularizadas.

---

## 🏗️ Arquitetura do Projeto

A solução está dividida em dois projetos principais para simular um ambiente cliente-servidor real:

1. **`GrpcService` (Servidor):**
   * **`Protos/`**: Contém os contratos de interface fortemente tipados (`greet.proto`).
   * **`Services/`**: Implementação dos serviços gRPC (`GreeterService` e `TokenService`).
   * **`Repositories/`**: Camada de persistência em memória thread-safe (`IClienteRepository`).
   * **`Interceptors/`**: Interceptadores globais de requisição (`JwtAuthInterceptor` e `ExceptionInterceptor`).
   * **`Validators/`**: Regras de validação de entrada declarativas com *FluentValidation*.
   * **`Extensions/`**: Configuração limpa e desacoplada do container de injeção de dependência (`DependencyInjectionExtensions`).

2. **`GrpcClient` (Cliente):**
   * Aplicação CLI interativa que consome o servidor gRPC utilizando autenticação baseada em token Bearer (`Metadata`), realizando login, cadastros, consultas e reajustes salariais com tratamento seguro de exceções (`RpcException`).

---

## 🔒 Recursos de Segurança Implementados

* **Autenticação baseada em Token (JWT Bearer):** O cliente realiza o login prévio enviando credenciais; o servidor valida e assina um token HMAC-SHA256 (configurado via `appsettings.json`), que é injetado automaticamente via cabeçalho `Metadata` em todas as chamadas protegidas.
* **Autorização por Interceptor:** O `JwtAuthInterceptor` intercepta chamadas de rotas protegidas antes de atingirem a regra de negócio, validando a integridade, o emissor, a audiência e a expiração do token.
* **Tratamento de Exceções Global (Fail-Safe):** O `ExceptionInterceptor` captura falhas críticas ou violações de regras, evitando vazamento de stack traces e convertendo-as em códigos padrão gRPC (`NotFound`, `InvalidArgument`, `Unauthenticated`).
* **Validação Rigorosa de Entrada:** O *FluentValidation* barra dados inconsistentes logo na borda do servidor, garantindo integridade transacional.
* **Rate Limiting Nativo:** Proteção de endpoints contra abuso e ataques de negação de serviço (DoS) configurada com políticas de janela fixa (*Fixed Window*).

---

## ⚙️ Como Executar o Projeto

### Pré-requisitos
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado em sua máquina.

### Passos para Execução (Desenvolvimento Local)

1. Clone o repositório:
   ```bash
   git clone [https://github.com/seu-usuario/seu-repositorio.git](https://github.com/seu-usuario/seu-repositorio.git)
   cd seu-repositorio
   ```
   
2. Restaure as dependências
	```bash
	dotnet restore
	dotnet build   
	```
	
3. Executando no servidor
	```bash
	cd GrpcService
	dotnet run
	```
	(O servidor escutará por padrão em https://localhost:7141)
	
4. Executando um client
	```bash
	cd GrpcClient
	dotnet run
	```
	
🕹️ Guia de Uso Rápido (Cliente Interativo)
Ao iniciar o cliente, o fluxo de segurança exige autenticação imediata:

Insira o usuário e senha padrão de teste presentes no código. 

Após a emissão e validação bem-sucedida do token JWT, o menu interativo será exibido:

1. Cadastrar novo cliente (com validação de dados de entrada).

2. Consultar cliente por ID (com tratamento de erro para IDs inexistentes).

3. Atualizar salário (com reajuste percentual validado).

0. Sair da aplicação.

💡 Pontos Positivos e Diferenciais desta Arquitetura
Separação de Conceitos (SoC): Código altamente modular, facilitando testes unitários e manutenção a longo prazo.

Resiliência e Observabilidade: Logs detalhados de requisições, fallbacks estruturados e códigos de status padronizados pela especificação oficial do gRPC.

Pronto para Produção: Estrutura limpa de injeção de dependência e configurações desacopladas no appsettings.json.
