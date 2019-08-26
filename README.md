# DesafioPloomes

##API de manipulação de base de dados

 
A Ploomes precisa de um sistema de contratação eficiente, em que os candidatos são avaliados online e é realizada uma triagem com base nos resultados antes das entrevistas. Para tal sistema, os seguintes requisitos foram levantados:


## Entidades

* Administrador (Nome, ApiKey)
* Candidato (Nome, ApiKey)
* Vaga (Nome da vaga, descrição)
* Questão (Nome da questão, respostas, resposta certa)
* Avaliação (Nome da avaliação, questões, data e hora de início, data e hora de fim)


## Precisamos que o usuário administrador possa:

* Cadastrar, atualizar ou excluir uma vaga

* Cadastrar, atualizar ou excluir uma avaliação

* Cadastrar, atualizar ou excluir uma questão

 

## A estrutura do banco e as chamadas da API devem permitir que o administrador extraia informações como:

* Para que vaga X candidato se candidatou?

* Qual foi a nota de X na avaliação?

* Quem foi o melhor candidato para a vaga Y?

 

## O candidato precisa extrair informações como:

* Quais vagas eu me candidatei?

* Qual foi minha nota nas minhas avaliações?

 

#### É obrigatório que exista uma validação em todas as chamadas para que candidatos não vejam informações de outros.

 

## A avaliação do desafio consiste em:

* Cumprimento dos requisitos

* Clareza e organização do código

* Cumprimento do prazo


O desafio pode ser cumprido utilizando qualquer linguagem de programação com qualquer banco de dados.

O código fonte deve ser entregue até às 23h59 do dia 07/03/2018 via GitHub ou e-mail. Utilize qualquer serviço de hospedagem para deixar o sistema online. 

Boa sorte!
