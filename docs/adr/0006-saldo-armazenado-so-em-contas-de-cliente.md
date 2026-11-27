# 0006 — Saldo armazenado só nas contas de cliente

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

O ledger tem dois tipos de conta:

- **contas de cliente** (depósitos, `4.1.1.NNNNNN`): cada uma é movimentada por um único cliente e não pode ficar com saldo negativo;
- **contas internas** (por exemplo, a conta de liquidação `1.1.2`): participam de **todas** as operações. Todo depósito e todo saque de qualquer cliente gera uma partida nelas.

Para garantir que um saque não deixe a conta negativa com requisições simultâneas, o saldo precisa ser lido e gravado de forma consistente. O projeto usa concorrência otimista com `rowversion` ([ADR 0002](0002-sql-server-como-banco.md)): se duas transações alteram a mesma linha, a segunda falha e tenta de novo.

## Decisão

**Só as contas de cliente guardam o saldo corrente**, numa coluna atualizada a cada lançamento e protegida por `rowversion`.

- O saldo das contas internas **não é armazenado**. Ele é calculado somando as partidas: débitos menos créditos ou o contrário, conforme a natureza da conta.
- No domínio, `Account.Apply` só altera o saldo em contas do tipo `CustomerDeposit`. Em contas internas, apenas valida que a conta está ativa.
- Contas internas não são alteradas depois de criadas, então nunca entram em conflito de concorrência.

## Alternativas consideradas

- **Saldo armazenado em todas as contas:** toda operação de qualquer cliente atualizaria a linha da conta de liquidação. Duas requisições paralelas de clientes diferentes colidiriam nessa linha, e uma delas teria de repetir a transação. Com volume, a conta interna vira um gargalo: é o problema conhecido como **hot account**.
- **Saldo sempre calculado, inclusive para clientes:** elimina a coluna de saldo, mas a regra "não pode ficar negativo" exigiria somar o histórico dentro de uma transação com bloqueio a cada saque. É mais lento e mais difícil de garantir sob concorrência.
- **Contas internas divididas em sub-contas (sharding):** reduz a disputa espalhando as partidas por várias linhas. Resolve a escala, mas é uma complexidade que o volume deste projeto não justifica.

## Consequências

- Requisições de clientes diferentes não disputam nenhuma linha. A concorrência otimista só atua onde há conflito real: dois saques na mesma conta de cliente.
- O saldo de uma conta interna custa uma consulta de soma. Ela é indexada por conta e por data contábil, e é usada na conciliação e no relatório, que não estão no caminho de cada operação.
- Duas formas de obter saldo convivem: o campo armazenado (clientes) e o cálculo pelas partidas (internas e saldos em datas passadas). A conciliação da fase 2 confere que as duas formas batem.
