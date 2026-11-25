# 0001 — Monolito modular na fase 1

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

O sistema tem três partes que conversam entre si: ledger contábil, conciliação e relatório regulatório de crédito. A fase 1 entrega só o ledger, e a conciliação deve virar um serviço próprio na fase 2. O projeto visa demonstrar arquitetura de sistemas e aplicações distribuídas sem pagar o custo operacional de microsserviços desde o primeiro dia.

## Decisão

A fase 1 começa como um **monolito modular**: um único processo (`CoreBanking.Api`) que hospeda módulos independentes.

- Cada módulo tem **quatro projetos**:
  - `Domain` (C# puro, sem pacotes de infraestrutura);
  - `Application` (casos de uso e interfaces de persistência);
  - `Infrastructure` (EF Core, mensageria, clientes externos);
  - `Contracts` (eventos e DTOs públicos).
- Dependências permitidas: `Infrastructure → Application → Domain`, com `Application → Contracts`. O host compõe tudo por injeção de dependência.
- Um módulo só referencia o projeto `Contracts` de outro módulo, nunca o domínio ou a infraestrutura dele.
- Cada módulo tem seu **schema** no banco (`ledger`, `credit`, `reconciliation`) e seu próprio `DbContext`. Nenhum módulo lê tabelas de outro.
- O ledger se comunica com a conciliação **sempre por mensagens** (outbox + RabbitMQ), mesmo enquanto os dois rodam no mesmo processo.
- Projetos de um módulo só são criados na fase em que o módulo é implementado.

## Alternativas consideradas

- **Microsserviços desde a fase 1:** exige broker, vários bancos e vários deploys antes de existir uma regra de negócio. O custo cai antes do benefício, e as fronteiras seriam chutadas sem conhecer o domínio.
- **Um projeto por módulo, com camadas em pastas:** metade dos projetos, mas a regra "o domínio não depende de EF Core" passaria a depender de convenção e teste de arquitetura, não do compilador.
- **Vertical slices num projeto único:** boa para CRUDs, mas não separa domínio e camadas, que é o que o projeto quer demonstrar.

## Consequências

- O compilador impede que o domínio use infraestrutura: o `Domain.csproj` não tem o pacote do EF Core.
- A extração da conciliação (fase 2) vira criar um host novo, mover o registro do módulo e dar a ele um banco próprio. Ela será registrada num ADR próprio.
- Mais projetos para navegar: cerca de 12 ao fim da fase 3.
- A comunicação por mensagens dentro do mesmo processo parece cerimônia na fase 2, mas é ela que torna a extração barata.
