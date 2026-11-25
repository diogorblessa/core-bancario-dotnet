# 0004 — .NET 11 em vez do .NET 10 LTS

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

O projeto começa do zero, sem código legado nem dependências presas a uma versão do .NET. Na data de início existem duas opções suportadas:

- **.NET 10**, versão LTS lançada em novembro de 2025;
- **.NET 11**, versão STS lançada em novembro de 2026, junto com o C# 15.

Até 2025, uma versão STS tinha 18 meses de suporte, e escolher LTS era quase automático. Em setembro de 2025 a Microsoft passou o suporte das versões STS para **24 meses**. Com isso, o .NET 11 é suportado até **novembro de 2028**, a mesma data de fim de suporte do .NET 10 LTS.

## Decisão

Usaremos o **.NET 11** com o **C# 15**, fixado no `global.json` (SDK `11.0.100`, `rollForward: latestFeature`).

- O prazo de suporte é o mesmo do .NET 10. Começar numa versão mais antiga não traria ganho de suporte.
- O C# 15 traz **union types**: um valor que é exatamente um entre vários casos, com o `switch` verificado pelo compilador. Isso ajuda a modelar o domínio do ledger, por exemplo o resultado de um lançamento (aceito, saldo insuficiente, período fechado), sem depender só de exceções.

## Alternativas consideradas

- **.NET 10 LTS:** é a escolha padrão em empresas, especialmente em bancos, que costumam padronizar em versões LTS para alinhar ciclos de atualização entre vários sistemas. Aqui esse benefício não existe: há um único sistema e o fim de suporte é o mesmo. Ficaríamos sem o C# 15 sem ganhar nada em troca.

## Consequências

- O projeto usa a versão mais recente da plataforma e da linguagem, com o mesmo horizonte de suporte do LTS.
- Algumas bibliotecas podem demorar a declarar suporte oficial ao .NET 11. Como as versões do .NET são compatíveis entre si, pacotes feitos para o .NET 10 costumam funcionar normalmente.
- Num time real, esta decisão seria discutida com o time, considerando a política de versões da empresa. O critério deste ADR vale para um projeto novo e isolado.
- A próxima LTS é o .NET 12, prevista para novembro de 2027. Rever esta decisão antes do fim do suporte, em novembro de 2028.
