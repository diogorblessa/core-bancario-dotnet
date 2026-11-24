# 0002 — SQL Server como banco de dados

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

O ledger precisa de transações ACID: uma operação grava, numa única transação, a transação contábil, as partidas, o saldo da conta de cliente, o registro de idempotência e a mensagem de outbox. Precisa também de concorrência otimista, para que dois saques simultâneos não deixem a conta negativa, e de índices únicos, que são a base da idempotência. O projeto mira vagas em times de banking e reporte regulatório, onde o ecossistema .NET corporativo é comum. A restrição é usar só ferramentas gratuitas, rodando com `docker compose up`.

## Decisão

Usaremos **SQL Server 2025 Developer Edition** em container (`mcr.microsoft.com/mssql/server:2025-latest`), acessado via **EF Core** com o provedor `Microsoft.EntityFrameworkCore.SqlServer`.

- A concorrência otimista usa uma coluna **`rowversion`**, que o banco atualiza a cada `UPDATE` e o EF Core usa como token de concorrência.
- Os testes de integração usam SQL Server real via **Testcontainers**. O provedor em memória do EF Core não é usado, porque não tem transações nem `rowversion`.

## Alternativas consideradas

- **PostgreSQL:** gratuito, leve e excelente. A concorrência otimista usaria a coluna de sistema `xmin`. É menos presente no alvo do projeto (.NET em bancos) e não teria ganho técnico para este caso.
- **SQLite:** ótimo para testes rápidos, mas não tem concorrência real entre conexões nem `rowversion`. Os testes que provam idempotência e concorrência perderiam o sentido.
- **EF Core InMemory nos testes:** descartado pelo mesmo motivo; daria falsa confiança.

## Consequências

- `rowversion` nativo simplifica o controle de concorrência.
- A imagem é pesada (cerca de 2 GB de RAM em execução), então o Docker Desktop precisa de memória suficiente.
- A edição Developer não pode ser usada em produção. Isso é relevante para o deploy curto na AWS (fase 4), que vai decidir entre RDS SQL Server Express ou não publicar o banco.
- Os testes de integração exigem Docker, localmente e no CI. Os runners Linux do GitHub Actions já têm Docker.
