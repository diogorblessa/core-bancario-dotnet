# 0005 — Data contábil informada com fechamento mensal

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

Todo lançamento do ledger tem duas datas:

- a **data contábil** (`BookingDate`), o dia a que o fato pertence;
- o **momento do registro** (`RecordedAt`), quando o sistema gravou o lançamento.

Na fase 3, o relatório regulatório de crédito é gerado a partir dos saldos do ledger numa data-base. Para que gerar o mesmo relatório duas vezes dê os mesmos números, os lançamentos daquele mês não podem mudar depois do envio. Ao mesmo tempo, os dados fictícios de demonstração precisam de uma carteira com histórico de vários meses.

## Decisão

A **data contábil é informada por quem lança**, e os **períodos contábeis fecham mês a mês**.

- A data contábil não pode estar no futuro. "Hoje" é calculado no fuso `America/Sao_Paulo`, a partir de um `TimeProvider` injetado, e nunca com `DateTime.Now`.
- A data contábil não pode cair num período já fechado.
- Os períodos fecham em ordem: novembro só fecha depois de outubro. Um período só fecha depois que o mês terminou.
- Um período fechado não reabre. Um erro num mês fechado é corrigido com estorno, que recebe a data de hoje e entra no período aberto.
- O relatório regulatório só é gerado para meses já fechados.

## Alternativas consideradas

- **Data contábil sempre igual a hoje:** é o modelo mais simples. Para montar uma carteira com histórico, porém, a API precisaria de uma porta de "viagem no tempo", e o relatório de um mês passado continuaria mutável.
- **Data informada, sem fechamento:** permite o histórico, mas um lançamento retroativo mudaria um relatório já gerado. O relatório deixaria de ser reproduzível.

## Consequências

- O relatório de um mês fechado é reproduzível: gerar de novo dá os mesmos números.
- A data contábil e o momento do registro ficam separados. Isso é o que permite auditar um lançamento retroativo dentro do período aberto.
- O fuso entra na regra. Às 01h30 UTC do dia 25 ainda é dia 24 em São Paulo, então um lançamento com data 25 seria recusado como futuro. Esse caso tem teste próprio.
- O ledger ganha um calendário contábil e um endpoint para fechar períodos. Fechar o mês vira um passo operacional explícito.
