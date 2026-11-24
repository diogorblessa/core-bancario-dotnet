# 0003 — Floci como emulador de AWS

- **Status:** Aceito
- **Data:** 2026-11-24

## Contexto

Na fase 3, o worker do relatório regulatório envia o arquivo gerado para o Amazon S3. Na fase 4, o frontend lista e baixa esses arquivos. Em desenvolvimento e no CI, o S3 precisa ser emulado localmente, sem custo e sem conta. A regra do projeto é que quem clonar o repositório suba tudo com um único `docker compose up`.

Desde a versão 2026.03, o LocalStack exige auth token para iniciar. O plano gratuito (Hobby) exige criar conta e é restrito a uso não comercial.

## Decisão

Usaremos o **Floci** (licença MIT, sem conta e sem token) como emulador de AWS, com a **versão da imagem fixada** no `docker-compose.yml`.

- O código usa apenas o SDK oficial (`AWSSDK.S3`). O endereço do serviço (`S3:ServiceUrl`, por exemplo `http://floci:4566`) e o `ForcePathStyle` vêm de configuração.
- O bucket é criado por um container de inicialização com o AWS CLI, não pela aplicação, como seria na AWS real.

## Alternativas consideradas

- **LocalStack Hobby:** a opção mais conhecida e madura, mas exige conta e token em todo ambiente. Isso quebra o "um único `docker compose up`" para quem avalia o repositório, e no CI público os PRs vindos de fork não têm acesso a segredos. Fixar uma imagem antiga da edição comunitária funciona hoje, mas ela não recebe mais correções.
- **fakecloud:** sem conta, mas licença AGPL, projeto recente com vários forks e números de cobertura só autodeclarados.
- **MinIO:** S3 compatível e maduro, mas a edição comunitária deixou de publicar imagens Docker no fim de 2025.
- **S3Mock:** maduro e focado só em S3. Fica como **plano B** se o Floci apresentar problemas.

## Consequências

- Clonar e subir funciona sem conta, localmente e no CI.
- O Floci é um projeto novo (1.x), com risco de bugs ou de mudanças de comportamento. A versão fixada e o uso do SDK oficial, com o endpoint em configuração, tornam a troca de emulador uma mudança de configuração, sem alterar código.
- Emuladores não reproduzem tudo da AWS real (permissões IAM, mensagens de erro, consistência). O deploy curto da fase 4 valida o comportamento real.
- Rever esta decisão se o Floci for descontinuado ou se a fase 4 precisar de serviços que ele não emula bem.
