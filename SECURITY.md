# Segurança — Kessler API

> Documento de Cybersecurity elaborado para a Global Solution 2026 — FIAP.  
> Cobre os pilares: Análise de Riscos, Arquitetura de Segurança, Governança, Continuidade,
> Validação de Entrada, Proteção de APIs, Segurança de Dados e Monitoramento/Auditoria.

---

## 1. Análise de Riscos e Ameaças (Threat Modeling)

### 1.1 Identificação de Ativos

Os ativos críticos gerenciados pela Kessler API são classificados em dados e infraestrutura:

| Ativo | Classificação | Criticidade |
|-------|--------------|-------------|
| Dados de objetos orbitais (posição, velocidade, NORAD ID, massa, risco) | Dado operacional | Alta |
| Cenários de missão (tipo, datas, status, progresso) | Dado operacional | Alta |
| Estimativas de reaproveitamento de materiais | Dado operacional | Média |
| Credenciais de usuários (e-mail + hash de senha) | Dado pessoal sensível | Alta |
| Tokens JWT em circulação | Credencial de sessão | Alta |
| Chave secreta JWT (`KesslerSuperSecretKey`) | Segredo de aplicação | Crítica |
| Connection string do Oracle (host, porta, usuário, senha) | Segredo de infraestrutura | Crítica |
| Banco de dados Oracle (`KESSLER.*`) | Infraestrutura de dados | Crítica |
| Logs estruturados da API (ILogger) | Dado de auditoria | Média |
| Endpoints REST da API (endereço e porta expostos) | Superfície de ataque | Média |

### 1.2 Modelo de Ameaças

Foram identificados os seguintes vetores de ataque plausíveis para a solução:

#### Vetor 1 — Interceptação de Tokens JWT (Man-in-the-Middle)
- **Descrição:** Um atacante posicionado entre o cliente e a API pode capturar tokens JWT transmitidos em texto claro caso a comunicação não use TLS.
- **Impacto:** Acesso não autorizado a todos os endpoints protegidos com as permissões do usuário capturado.
- **Controle:** Uso obrigatório de HTTPS/TLS (reforçado pelo header `Strict-Transport-Security`). Tokens com expiração de 60 minutos reduzem a janela de exploração.

#### Vetor 2 — Brute Force / Credential Stuffing no Login
- **Descrição:** Atacante tenta combinações de e-mail/senha no endpoint `POST /api/auth/login` de forma automatizada, aproveitando credenciais vazadas de outros serviços.
- **Impacto:** Comprometimento de contas de usuários, especialmente `Admin`.
- **Controle:** Rate Limiting de 100 requisições/minuto por IP (FixedWindow). Senhas armazenadas com BCrypt (hash lento), tornando ataques offline inviáveis. Middleware de exceções não revela se o e-mail existe ou não.

#### Vetor 3 — Injeção em Dados Orbitais (SQL Injection / Data Tampering)
- **Descrição:** Atacante envia payloads maliciosos nos campos de criação/atualização de objetos orbitais (ex: NORAD ID, descrição) tentando injeção SQL ou manipulação de telemetria.
- **Impacto:** Corrupção de dados orbitais, podendo comprometer a tomada de decisão de missões espaciais críticas.
- **Controle:** Entity Framework Core utiliza parametrização automática de todas as queries, eliminando SQL Injection. Validação de domínio via exceções (`InvalidOrbitalDataException`, `DuplicateNoradIdException`) rejeita dados inválidos antes de persistir.

#### Vetor 4 — Negação de Serviço (DDoS / Abuso de API)
- **Descrição:** Atacante sobrecarrega a API com volume massivo de requisições, tornando o sistema indisponível para usuários legítimos.
- **Impacto:** Indisponibilidade do sistema de monitoramento orbital, comprometendo missões em andamento.
- **Controle:** Rate Limiting nativo do ASP.NET Core (100 req/min/IP). Resposta padronizada `429 Too Many Requests` sem exposição de detalhes internos.

#### Vetor 5 — Escalada de Privilégios
- **Descrição:** Usuário com role `Operator` tenta acessar endpoints restritos a `Admin` (ex: `DELETE /api/orbitalobjects/{id}`).
- **Impacto:** Exclusão não autorizada de dados orbitais ou de missões críticas.
- **Controle:** Autorização baseada em roles via `[Authorize(Roles = "Admin")]` nos endpoints sensíveis. O JWT carrega a claim de role gerada no momento do login, imutável durante a sessão.

---

## 2. Arquitetura de Segurança (Controles)

### 2.1 Controles de Acesso

A Kessler API adota um modelo de autenticação e autorização em duas camadas:

**Autenticação — JWT Bearer (HS256)**
- Todo cliente deve autenticar-se via `POST /api/auth/login`, recebendo um token JWT assinado com HMAC-SHA256.
- O token carrega as claims: `sub` (e-mail), `role` (`Admin` ou `Operator`) e `exp` (expiração em 60 minutos).
- Todos os endpoints, exceto `/api/auth/register` e `/api/auth/login`, exigem o header `Authorization: Bearer <token>`.

**Autorização — Role-Based Access Control (RBAC)**

| Role | Permissões |
|------|-----------|
| `Operator` | GET, POST, PUT, PATCH em todos os recursos |
| `Admin` | Todas as permissões de Operator + DELETE em qualquer recurso |

**Princípio do Mínimo Privilégio**  
Operadores do sistema de monitoramento orbital não têm acesso à exclusão de dados. Apenas administradores autorizados podem remover objetos orbitais, missões ou estimativas de materiais.

### 2.2 Proteção de Dados

**Dados em Trânsito**
- O header `Strict-Transport-Security: max-age=31536000; includeSubDomains` força HTTPS em todas as conexões por 1 ano.
- O header `Content-Security-Policy` impede carregamento de recursos externos não autorizados.
- Tokens JWT são transmitidos apenas via HTTPS, com expiração configurável (padrão: 60 minutos).

**Dados em Repouso**
- Senhas de usuários nunca são armazenadas em texto claro — são persistidas como hashes BCrypt (algoritmo lento, resistente a ataques de dicionário e rainbow tables).
- A chave secreta JWT (`KesslerSuperSecretKey2026!FIAP@GS#AES256Bits`) deve ser gerenciada via variáveis de ambiente ou cofre de segredos (ex: Azure Key Vault, AWS Secrets Manager) em ambientes de produção — nunca em código-fonte.
- A connection string do Oracle deve igualmente ser externalizada para variáveis de ambiente em produção.

**Proteção contra Vazamento de Informações**
- O `ExceptionMiddleware` centraliza o tratamento de erros: exceções de domínio retornam mensagens controladas (ex: `"Objeto orbital não encontrado"`) sem jamais expor stack traces, queries SQL ou detalhes de infraestrutura.
- O header `Server` é removido das respostas HTTP, ocultando informações sobre o servidor web.
- O header `X-Content-Type-Options: nosniff` impede MIME-sniffing pelo navegador.

### 2.3 Segurança da Infraestrutura

**Security Headers (implementados via `SecurityHeadersMiddleware`)**

| Header | Valor | Objetivo |
|--------|-------|----------|
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` | Força HTTPS |
| `X-Frame-Options` | `DENY` | Impede clickjacking |
| `Content-Security-Policy` | `default-src 'self'` | Bloqueia recursos externos |
| `X-XSS-Protection` | `1; mode=block` | Proteção XSS legada |
| `X-Content-Type-Options` | `nosniff` | Impede MIME sniffing |
| `Server` | *(removido)* | Oculta tecnologia do servidor |

**Rate Limiting**  
Implementado via `Microsoft.AspNetCore.RateLimiting` com política `FixedWindow`: 100 requisições por minuto por endereço IP. Respostas bloqueadas retornam `429 Too Many Requests`.

**Monitoramento e Logs**  
`ILogger` estruturado está integrado em todos os serviços de aplicação (`OrbitalObjectService`, `MissionService`, `AuthService`, etc.). Em produção, os logs devem ser direcionados a uma plataforma de SIEM (ex: Elasticsearch, Azure Monitor) para detecção de anomalias.

**Proteção contra Injeção**  
Entity Framework Core utiliza queries parametrizadas por padrão, eliminando a possibilidade de SQL Injection em todas as operações de repositório.

**Banco de Dados Oracle**  
O schema `KESSLER` segrega os dados da API dos dados do sistema Oracle. O usuário de banco tem permissões mínimas necessárias para operação. Em produção, recomenda-se revogar o role `DBA` e conceder apenas privilégios específicos de tabela.

---

## 3. Governança e Compliance

### 3.1 Alinhamento com a ISO 27001

A Kessler API foi desenvolvida alinhada aos controles da norma ISO/IEC 27001:2022, conforme mapeamento abaixo:

| Anexo A — Controle ISO 27001 | Implementação na Kessler API |
|------------------------------|------------------------------|
| **A.5 — Políticas de Segurança** | Este documento formaliza a política de segurança do sistema |
| **A.8 — Gestão de Ativos** | Seção 1.1 deste documento cataloga todos os ativos críticos |
| **A.9 — Controle de Acesso** | JWT Bearer + RBAC (`Admin`/`Operator`) + princípio do mínimo privilégio |
| **A.10 — Criptografia** | BCrypt para senhas; HS256 para tokens; TLS para dados em trânsito |
| **A.12 — Segurança nas Operações** | Rate limiting, ILogger estruturado, monitoramento de logs |
| **A.13 — Segurança nas Comunicações** | HTTPS obrigatório (HSTS), remoção de headers que expõem informações |
| **A.14 — Segurança no Desenvolvimento** | Clean Architecture com separação de camadas; exceções tipadas e controladas; sem exposição de internals |
| **A.16 — Gestão de Incidentes** | ExceptionMiddleware + Plano de Resposta a Incidentes (Seção 4) |
| **A.18 — Conformidade** | Aderência à LGPD (Seção 3.2) |

**Gestão de Riscos (baseada na ISO 27001)**  
Os riscos identificados no Modelo de Ameaças (Seção 1.2) foram avaliados conforme a metodologia da norma:
- **Probabilidade × Impacto** determina a criticidade de cada vetor.
- Controles técnicos foram implementados para todos os vetores de risco Alto e Crítico.
- Revisão periódica de riscos deve ocorrer a cada novo ciclo de desenvolvimento ou após qualquer incidente de segurança.

### 3.2 Privacidade e LGPD

A Lei Geral de Proteção de Dados (Lei 13.709/2018) é observada nos seguintes aspectos:

**Dados Pessoais Coletados**  
A Kessler API coleta o mínimo necessário para autenticação:
- E-mail do usuário (identificador único)
- Senha (armazenada exclusivamente como hash BCrypt — dado original nunca persiste)
- Role de acesso (`Admin` ou `Operator`)

Os dados operacionais (objetos orbitais, missões, materiais) são dados técnicos/científicos, não dados pessoais.

**Princípios da LGPD Aplicados**

| Princípio (Art. 6º LGPD) | Aplicação |
|--------------------------|-----------|
| **Finalidade** | Dados de usuário coletados exclusivamente para autenticação e controle de acesso ao sistema de monitoramento orbital |
| **Necessidade** | Coleta mínima: apenas e-mail, senha e role — sem dados de localização, comportamentais ou sensíveis além do necessário |
| **Adequação** | O tratamento é compatível com a finalidade declarada (acesso ao sistema) |
| **Segurança** | BCrypt, HTTPS/TLS, JWT com expiração — proteção técnica dos dados pessoais |
| **Prevenção** | Rate limiting e headers de segurança reduzem superfície de ataque |
| **Não Discriminação** | O sistema não realiza perfilamento, scoring pessoal nem decisões automatizadas sobre os usuários |

**Direitos do Titular**  
- Os usuários podem ter suas contas removidas mediante solicitação ao administrador do sistema (endpoint `DELETE /api/auth/{id}` sob role `Admin`).
- Nenhum dado pessoal é compartilhado com terceiros ou exportado fora do sistema.

---

## 4. Plano de Resiliência e Continuidade

### 4.1 Plano de Resposta a Incidentes

Este plano descreve os procedimentos a serem seguidos caso a Kessler API sofra uma invasão, violação de dados ou comprometimento de credenciais.

---

#### FASE 1 — CONTENÇÃO (primeiras 2 horas)

**Objetivo:** Impedir que o atacante avance ou cause danos adicionais.

1. **Detectar o incidente**
   - Revisar logs estruturados da API em busca de padrões anômalos: volume elevado de requisições de um IP, tentativas de acesso a endpoints não autorizados, erros 401/403 em massa.
   - Identificar o usuário ou token comprometido.

2. **Isolar o sistema**
   - Colocar a API em modo de manutenção (retornar `503 Service Unavailable` para todos os endpoints).
   - Bloquear IPs suspeitos no nível de firewall/load balancer.

3. **Revogar credenciais comprometidas**
   - Rotacionar imediatamente a chave secreta JWT (`Jwt:Key` no `appsettings.json` ou variável de ambiente). Todos os tokens em circulação tornam-se inválidos instantaneamente.
   - Alterar a senha do usuário Oracle (`kessler`) na connection string.
   - Se credenciais de usuário foram expostas: forçar reset de senha de todos os usuários afetados.

4. **Preservar evidências**
   - Exportar e preservar todos os logs da API antes de qualquer reinicialização.
   - Registrar timestamp, IPs envolvidos e endpoints acessados para análise forense.

---

#### FASE 2 — ERRADICAÇÃO (horas 2 a 24)

**Objetivo:** Identificar e eliminar completamente a causa raiz do incidente.

1. **Análise forense dos logs**
   - Determinar o vetor de ataque utilizado (ex: token roubado, brute force, injeção).
   - Identificar quais dados foram acessados, modificados ou exfiltrados.

2. **Corrigir a vulnerabilidade**
   - Se explorada por injeção: revisar e reforçar validações nas camadas de Application e Domain.
   - Se explorada por token comprometido: revisar tempo de expiração e implementar blocklist de tokens (Redis ou tabela de tokens revogados).
   - Se explorada por credencial de usuário fraca: implementar política de senha mínima no `AuthService`.

3. **Varredura de integridade**
   - Comparar estado atual do banco de dados com o último backup íntegro.
   - Identificar registros inseridos, modificados ou excluídos de forma não autorizada.
   - Reverter alterações indevidas via restauração seletiva de backup.

4. **Revogar e recriar acessos**
   - Invalidar todos os usuários criados durante o período do incidente.
   - Recriar usuários legítimos com novas credenciais.

---

#### FASE 3 — RECUPERAÇÃO (horas 24 a 72)

**Objetivo:** Restaurar a operação normal de forma segura e monitorada.

1. **Restaurar o sistema**
   - Reimplantar a API com a nova chave JWT e connection string atualizados.
   - Validar integridade do banco Oracle: checar constraints, sequências e dados críticos de objetos orbitais e missões.
   - Reativar endpoints gradualmente (iniciar com leitura, depois escrita).

2. **Monitoramento intensivo (72 horas)**
   - Aumentar nível de log para `Debug` temporariamente.
   - Configurar alertas automáticos para: picos de taxa de requisições, erros 4xx em sequência, acessos fora do horário padrão.

3. **Comunicação**
   - Notificar administradores e usuários afetados sobre o incidente, os dados potencialmente expostos e as ações tomadas — conforme exigido pelo Art. 48 da LGPD (comunicação ao titular e à ANPD em caso de violação que possa acarretar risco).

4. **Revisão pós-incidente**
   - Documentar lições aprendidas.
   - Atualizar este plano com os novos controles implementados.
   - Revisar o Modelo de Ameaças (Seção 1.2) para incorporar o vetor explorado.

---

#### Resumo do Plano

| Fase | Janela | Ações-chave |
|------|--------|-------------|
| **Contenção** | 0–2h | Isolar API, revogar tokens JWT, bloquear IPs, preservar logs |
| **Erradicação** | 2–24h | Análise forense, corrigir vulnerabilidade, verificar integridade dos dados |
| **Recuperação** | 24–72h | Restaurar serviço, monitoramento intensivo, notificar usuários afetados |

---

## 5. Segurança de Entrada e Validação de Dados

### 5.1 Validação de Entradas

Todos os DTOs da API utilizam Data Annotations do .NET para validação automática. O atributo `[ApiController]` retorna `400 Bad Request` com mensagem descritiva quando a validação falha, antes de qualquer execução de lógica de negócio.

| Campo | Validações aplicadas |
|-------|---------------------|
| `Name` (todos os recursos) | `[Required]`, `[MaxLength(200)]`, `[MinLength(2)]` |
| `Email` (auth) | `[Required]`, `[EmailAddress]`, `[MaxLength(200)]` |
| `Password` (register) | `[Required]`, `[MinLength(8)]`, `[MaxLength(128)]` |
| `NoradId` | `[MaxLength(20)]`, `[RegularExpression(@"^[A-Za-z0-9\-]*$")]` |
| `AltitudeKm` | `[Range(0, 1_000_000)]` |
| `InclinationDeg` | `[Range(-180, 180)]` |
| `EstimatedSharePct` | `[Range(0.0, 100.0)]` |
| `LaunchYear` | `[Range(1957, 2100)]` |
| `Summary` / `Notes` / `Objective` | `[MaxLength(...)]` (limites por campo) |

### 5.2 Sanitização contra XSS, SQL Injection e Command Injection

O `SanitizationMiddleware` inspeciona o corpo de toda requisição `application/json` antes de atingir os controllers. Se detectar qualquer padrão malicioso, retorna `400 Bad Request` com log de alerta e **nunca** processa o payload.

Padrões detectados:

| Categoria | Exemplos de padrões bloqueados |
|-----------|-------------------------------|
| **XSS** | `<script`, `javascript:`, `onerror=`, `<iframe`, `eval(`, `document.cookie` |
| **SQL Injection** | `UNION SELECT`, `DROP TABLE`, `'; --`, `xp_cmdshell`, `EXEC(`, `INFORMATION_SCHEMA` |
| **Command Injection** | `; rm -`, `| /bin/sh`, `&& curl`, `$(wget`, `` `curl `` |

Adicionalmente, o Entity Framework Core usa **queries parametrizadas** por padrão, tornando SQL Injection impossível mesmo que um padrão passe despercebido.

### 5.3 Limitação de Tamanho e Formato

- Tamanho máximo do corpo da requisição: **1 MB** (configurável via `RequestLimits:MaxBodySizeBytes`)
- Limite configurado diretamente no Kestrel (camada de servidor, antes do pipeline ASP.NET)
- Requisições que excedem o limite recebem `413 Request Entity Too Large`

### 5.4 Tratamento Seguro de Erros

O `ExceptionMiddleware` garante que nenhuma exceção não tratada expõe informações internas:
- Erros `5xx`: mensagem genérica `"Erro interno do servidor"`
- Erros de banco de dados: `"Erro ao persistir dados"` (sem query, sem schema)
- Stack traces: **nunca** expostos em nenhum ambiente
- Header `Server`: removido pelo `SecurityHeadersMiddleware`

---

## 6. Autenticação e Autorização

### 6.1 JWT com Assinatura Forte e Expiração

- Algoritmo: **HMAC-SHA256** com chave de 256 bits
- Expiração: 60 minutos (`ClockSkew = TimeSpan.Zero` — sem tolerância)
- Validações ativas: Issuer, Audience, Assinatura, Expiração
- Em produção: `RequireHttpsMetadata = true` (tokens só trafegam via HTTPS)

### 6.2 Complexidade de Senha

O `AuthService` valida a senha na criação de conta antes de gerar o hash BCrypt:

| Requisito | Regra |
|-----------|-------|
| Comprimento mínimo | 8 caracteres |
| Letras maiúsculas | Pelo menos 1 |
| Letras minúsculas | Pelo menos 1 |
| Números | Pelo menos 1 |
| Caractere especial | Pelo menos 1 (`!@#$%^&*` etc.) |

### 6.3 Controle de Acesso Baseado em Papéis (RBAC)

| Role | Permissões |
|------|-----------|
| `Operator` | GET, POST, PUT, PATCH em todos os recursos |
| `Admin` | Todas as permissões + DELETE em qualquer recurso |

Todas as falhas de autenticação retornam a mesma mensagem `"Credenciais inválidas"`, sem revelar se o e-mail existe ou não (prevenção de user enumeration).

---

## 7. Proteção de APIs e Serviços

### 7.1 HTTPS/TLS

- Header `Strict-Transport-Security: max-age=31536000; includeSubDomains` em todas as respostas
- `UseHttpsRedirection()` redireciona automaticamente HTTP → HTTPS
- `RequireHttpsMetadata = true` em produção (tokens JWT recusados em conexões sem TLS)

### 7.2 Rate Limiting e Throttling

- Política: **FixedWindow** — 100 requisições por minuto por IP
- Respostas bloqueadas: `429 Too Many Requests`
- Implementado via `Microsoft.AspNetCore.RateLimiting` (nativo .NET 7+)

### 7.3 CORS Configurado Corretamente

A política `AllowAll` foi substituída por `RestrictedCors`:
- Origens permitidas definidas em `appsettings.json` → `AllowedOrigins[]`
- Apenas as origens listadas recebem resposta com `Access-Control-Allow-Origin`
- Em desenvolvimento sem origens configuradas: fallback para AllowAny (apenas dev)
- Em produção: lista explícita obrigatória

### 7.4 Integridade de Payload (HMAC-SHA256)

O `PayloadIntegrityMiddleware` verifica a assinatura do corpo da requisição:
- Header: `X-Payload-Signature: <hex-hmac-sha256>`
- Algoritmo: HMAC-SHA256 com segredo configurado em `PayloadIntegrity:Secret`
- Comparação com `CryptographicOperations.FixedTimeEquals` (previne timing attacks)
- Habilitado via `PayloadIntegrity:Enabled = true` em `appsettings.json`
- Padrão desabilitado para facilitar uso via Swagger em desenvolvimento

---

## 8. Segurança de Dados e Privacidade

### 8.1 Dados Sensíveis em Repouso

- Senhas: hash **BCrypt** (algoritmo lento, salt automático, resistente a GPU attacks)
- Chave JWT: externalizada para variáveis de ambiente em produção
- Connection string Oracle: nunca exposta em logs ou respostas de erro

### 8.2 Mascaramento em Logs

E-mails em logs são mascarados para proteger dados pessoais: `ian@gmail.com` → `i**@gmail.com`.  
Senhas **nunca** aparecem em logs — apenas o hash é persistido e nenhuma operação de log inclui o campo `password`.

### 8.3 Proteção contra Exposição Acidental

- Todos os endpoints estão documentados no Swagger (sem endpoints ocultos)
- Exceções não expõem schema, queries SQL ou stack traces
- Header `Server` removido (não revela tecnologia do servidor)
- Logs do Entity Framework configurados em nível `Warning` (queries SQL não aparecem em produção)

### 8.4 Política de Retenção

- Dados podem ser removidos via `DELETE` (endpoints protegidos por role `Admin`)
- Contas de usuário podem ser desativadas (`User.Deactivate()`) sem exclusão imediata
- Em conformidade com LGPD: titular pode solicitar exclusão ao administrador

---

## 9. Monitoramento, Logs e Auditoria

### 9.1 Logs Estruturados e Seguros

O `ILogger` estruturado está integrado em todos os serviços. Regras de segurança nos logs:
- E-mails mascarados (`i**@gmail.com`)
- Senhas: **nunca** registradas
- Tokens JWT: **nunca** registrados
- IPs: registrados para fins de auditoria de segurança

### 9.2 Monitoramento de Eventos Suspeitos

O `AuthService` registra explicitamente:

| Evento | Log Level | Mensagem |
|--------|-----------|---------|
| E-mail inexistente | `Warning` | `[AUTH] Tentativa de login com e-mail inexistente` |
| Conta desativada | `Warning` | `[AUTH] Tentativa de login em conta desativada` |
| Senha incorreta | `Warning` | `[AUTH] Falha de autenticação — senha incorreta` |
| Login bem-sucedido | `Information` | `[AUTH] Login bem-sucedido` |

O `AuditMiddleware` registra `401 Unauthorized` e `403 Forbidden` com prefixo `[SECURITY]` para facilitar filtros em ferramentas de SIEM.

### 9.3 Trilha de Auditoria para Ações Críticas

O `AuditMiddleware` registra **toda operação mutante** (POST, PUT, PATCH, DELETE) com:

| Campo | Exemplo |
|-------|---------|
| Método + Path | `POST /api/orbitalobjects` |
| Status HTTP | `201` |
| UserId | ID do usuário no JWT |
| Role | `Operator` |
| Email | `i**@gmail.com` (mascarado) |
| IP | `192.168.1.10` |
| Tempo de resposta | `42ms` |

Exemplo de entrada de log de auditoria:
```
[AUDIT] POST /api/orbitalobjects → 201 | UserId: 5 | Role: Operator | Email: i**@gmail.com | IP: 192.168.1.10 | 42ms
```
