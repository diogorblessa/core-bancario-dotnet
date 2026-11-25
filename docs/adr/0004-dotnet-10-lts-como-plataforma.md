# 0004 — .NET 10 LTS como plataforma

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

O projeto começa do zero, sem código legado nem dependências presas a uma versão do .NET. Na data de início existem duas opções suportadas:

- **.NET 10**, versão LTS lançada em novembro de 2025, com o C# 14;
- **.NET 11**, versão STS lançada em novembro de 2026, com o C# 15.

Em setembro de 2025 a Microsoft passou o suporte das versões STS de 18 para **24 meses**. Com isso, as duas versões têm suporte até **novembro de 2028**, e o prazo de suporte deixou de ser um critério de desempate.

## Decisão

Usaremos o **.NET 10 LTS** com o **C# 14**, fixado no `global.json` (SDK `10.0.404`, `rollForward: latestFeature`).

- É a versão que bancos e outras empresas reguladas costumam padronizar. O ciclo LTS alinha as atualizações entre vários sistemas, e é o ambiente que um desenvolvedor encontra nesses times.
- Está no mercado há um ano. Bibliotecas, ferramentas e documentação já foram testadas nela, e há muito mais material de referência.
- O ganho concreto do .NET 11 seriam os **union types** do C# 15. Na fase 1, os erros de regra de negócio são tratados com exceções (`DomainException` → HTTP 422), então esse recurso não seria usado.

## Alternativas consideradas

- **.NET 11 STS:** mesmo prazo de suporte e acesso ao C# 15. Union types permitem modelar resultados do domínio (lançamento aceito, saldo insuficiente, período fechado) com o `switch` verificado pelo compilador. Foi a escolha inicial do projeto. Ficou de fora porque o recurso tinha acabado de sair, com pouco material e ferramentas ainda se ajustando, e porque o tratamento de erros por exceção é o padrão nos sistemas .NET em produção hoje.

## Consequências

- O projeto usa a versão que o mercado-alvo usa, com pacotes e exemplos alinhados a ela.
- Union types ficam fora do projeto. Se forem úteis mais adiante, por exemplo no módulo Credit da fase 3, a migração para uma versão mais nova será registrada num ADR próprio.
- A próxima LTS é o .NET 12, prevista para novembro de 2027. Rever esta decisão antes do fim do suporte do .NET 10, em novembro de 2028.
