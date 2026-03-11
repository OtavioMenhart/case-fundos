# Case de engenharia Itau - .Net

## Introdução
Neste projeto esta sendo utilizada a base de dados sqlite (arquivo dbcaseitau S3db) com as seguintes tabelas:

    Tabela: TIPO_FUNDO > "Tipos de fundos existentes"
	- CODIGO      - INT         NOT NULL - PRIMARY KEY
	- NOME        - VARCHAR(20) NOT NULL

    Tabela: FUNDO > "Registro relacionados ao cadastro de fundos"
	- CODIGO      - VARCHAR(20)  UNIQUE NOT NULL - PRIMARY KEY
	- NOME        - VARCHAR(100)        NOT NULL
	- CNPJ        - VARCHAR(14)  UNIQUE NOT NULL
	- CODIGO_TIPO - INT                 NOT NULL - FOREIGN KEY TIPO_FUNDO(CODIGO)
	- PATRIMONIO  - NUMERIC                 NULL

> Obs.: você pode fazer o uso do [sqliteadmin] para gerenciar a base de dados, visualizar as tabelas e seus respectivos dados

No projeto CaseItau.API foi disponibilizada uma API de Fundos com os metodos abaixo realizando acoes diretas na base de dados:

	GET                        - LISTAR TODOS OS FUNDOS CADASTRADOS
	GET    {CODIGO}            - RETORNAR OS DETALHES DE UM DETERMINADO FUNDO PELO CÓDIGO
	POST   {FUNDO}             - REALIZA O CADASTRO DE UM NOVO FUNDO
	PUT    {CODIGO}            - EDITA O CADASTRO DE UM FUNDO JÁ EXISTENTE
	DELETE {CODIGO}            - EXCLUI O CADASTRO DE UM FUNDO
	PUT    {CODIGO}/patrimonio - ADICIONA OU SUBTRAI DETERMINADO VALOR DO PATRIMONIO DE UM FUNDO

## Ações a serem realizadas
1. Faça o fork do projeto no seu github. Não realize commits na branch main e nem crie novas branchs.
2. O código da api de fundos faz mal uso dos objetos, não segue boas práticas e não possui qualidade. Refatore o codigo utilizando as melhores bibliotecas, praticas, patterns e garanta a qualidade da aplicação. Fique a vontade para usar outros componentes e base de dados.
2. Após a inclusão de um novo fundo via API, os metodos GET da API de Fundos estão retornando erro. Identifique e corrija o erro
3. Crie uma aplicação web (Angular ou ASP NET MVC) que consuma todos os metodos da API de fundos

Se a sua vaga for BACKEND não se preocupe em fazer a parte de FRONT.<br>
Após finalizar o case, envie o link do seu github com a solução final para o gestor que o solicitou.

[sqliteadmin]: <http://sqliteadmin.orbmu2k.de> 
