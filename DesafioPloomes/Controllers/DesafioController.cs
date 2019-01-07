using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;
using DesafioPloomes.Models;
using System.Text;

namespace DesafioPloomes.Controllers
{
    public class DesafioController : ApiController
    {

        public string ConnectionString = "Server=pinedos.database.windows.net;Database=Pinedos;User Id=IgnacioPinedo;Password=Igyigy97;";


        [HttpPost]
        [Route("Desafio/NovaVaga/{NomeVaga}/{Descricao}")]
        public IHttpActionResult NovaVaga(string NomeVaga, string Descricao, [FromBody] string hash)
        {

            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (NomeVaga == "" || NomeVaga == null || Descricao == "" || Descricao == null)
                return Ok(new { message = "Dados errados." });

            Vaga vaga = new Vaga();

            using (var conn = new SqlConnection(ConnectionString))
            {
                vaga.IdVaga = conn.Query<int>(@"IF EXISTS (SELECT *
                                                	 	   FROM tbVaga
                                                	 	   WHERE Vaga = @Vaga
                                                           AND Descricao = @Descricao)
                                                	BEGIN
                                                	     SELECT 0;
                                                	END
                                                ELSE
                                                    BEGIN
                                                	    INSERT INTO tbVaga
                                                		VALUES (@Vaga, @Descricao)
                                                        
                                                		SELECT idVaga IdVaga
                                                		FROM tbVaga
                                                		WHERE Vaga = @Vaga
                                                		AND Descricao = @Descricao
                                                	 END", new
                {
                    Vaga = NomeVaga,
                    Descricao = Descricao
                }).FirstOrDefault();
            }

            if (vaga.IdVaga == 0)
                return Ok(new { message = "Vaga ja existente." });

            vaga.NomeVaga = NomeVaga;
            vaga.Descricao = Descricao;

            return Ok(new
            {
                IdVaga = vaga.IdVaga,
                NomeVaga = vaga.NomeVaga,
                Descricao = vaga.Descricao
            });
        }

        [HttpPost]
        [Route("Desafio/AtualizarVaga/{idVaga}/{AtualizacaoNomeVaga}/{AtualizacaoDescricao}")]
        public IHttpActionResult AtualizarVaga(int idVaga, string AtualizacaoNomeVaga, string AtualizacaoDescricao, [FromBody] string hash)
        {

            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (AtualizacaoNomeVaga == "" || AtualizacaoNomeVaga == null || AtualizacaoDescricao == "" || AtualizacaoDescricao == null)
                return Ok(new { message = "Dados errados." });

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                                		   FROM tbVaga
                                                		   WHERE idVaga = @idVaga)
                                                	BEGIN
                                                		UPDATE V
                                                		SET Vaga = @Vaga
                                                		  , Descricao = @Descricao
                                                		FROM tbVaga V
                                                		WHERE V.idVaga = @idVaga
                                                
                                                		SELECT 1
                                                	END
                                                ELSE
                                                	BEGIN
                                                		SELECT 0
                                                	END", new
                {
                    idVaga = idVaga,
                    Vaga = AtualizacaoNomeVaga,
                    Descricao = AtualizacaoDescricao
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpDelete]
        [Route("Desafio/ApagarVaga/{IdVaga}")]
        public IHttpActionResult ApagarVagas(int IdVaga, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbVaga
                                            		   WHERE idVaga = @IdVaga)
                                            	BEGIN
													DELETE 
													FROM tbResposta
													WHERE idQuestao IN (SELECT Q.idQuestao
                                            						    FROM tbQuestao Q
                                            						    INNER JOIN tbAvaliacao A
                                            						    ON Q.idAvaliacao = A.idAvaliacao
                                            						    WHERE A.idVaga = @IdVaga) 

                                            		DELETE
                                            		FROM tbQuestao 
                                            		WHERE idAvaliacao IN (SELECT idAvaliacao
                                            						      FROM tbAvaliacao
                                            						      WHERE idVaga = @IdVaga)
                                            		
                                            		DELETE
                                            		FROM tbAvaliacao
                                            		WHERE idVaga = @IdVaga
                                            
                                            		DELETE
                                            		FROM tbVaga
                                            		WHERE idVaga = @IdVaga
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdVaga = IdVaga
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("Desafio/Vagas")]
        public IHttpActionResult Vagas([FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Vaga> vagas;

            using (var conn = new SqlConnection(ConnectionString))
            {
                vagas = conn.Query<Vaga>(@"SELECT idVaga IdVaga
                                                , Vaga NomeVaga
                                                , Descricao Descricao
                                           FROM tbVaga").ToList();
            }

            return Ok(vagas.Select(v => new
            {
                idVaga = v.IdVaga,
                NomeVaga = v.NomeVaga,
                Descricao = v.Descricao
            }));
        }

        [HttpPost]
        [Route("Desafio/NovaAvaliacao/{NomeAvaliacao}/{idVaga}")]
        public IHttpActionResult NovaAvaliacao(string NomeAvaliacao, int idVaga, [FromBody] AvaliacaoHash AvaliacaoHash)
        {
            User usuario = Autenticar(AvaliacaoHash.Hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (NomeAvaliacao == "" || NomeAvaliacao == null || AvaliacaoHash.Inicio == null || AvaliacaoHash.Fim == null || idVaga == 0)
                return Ok(new { message = "Dados errados." });

            Avaliacao avaliacao = new Avaliacao();

            using (var conn = new SqlConnection(ConnectionString))
            {
                avaliacao.IdAvaliacao = conn.Query<int>(@"IF EXISTS (SELECT *
                                                          	 	     FROM tbAvaliacao
                                                          	 	     WHERE Avaliacao = @NomeAvaliacao
                                                                     AND Inicio = @Inicio
												          		     AND Fim = @Fim
												          		     AND idVaga = @idVaga)
                                                              BEGIN
                                                          	      SELECT 0;
                                                              END
                                                          ELSE
                                                              BEGIN
                                                          	      INSERT INTO tbAvaliacao
												          		  VALUES (@NomeAvaliacao, @Inicio, @Fim, @idVaga)
                                                                  
												          		  SELECT idAvaliacao IdAvaliacao
                                                          		  FROM tbAvaliacao
                                                          	 	  WHERE Avaliacao = @NomeAvaliacao
                                                                  AND Inicio = @Inicio
												          		  AND Fim = @Fim
												          		  AND idVaga = @idVaga
												          	  END", new
                {
                    NomeAvaliacao = NomeAvaliacao,
                    Inicio = AvaliacaoHash.Inicio,
                    Fim = AvaliacaoHash.Fim,
                    idVaga = idVaga
                }).FirstOrDefault();
            }

            if (avaliacao.IdAvaliacao == 0)
                return Ok(new { message = "Avaliacao ja existente." });
            
            avaliacao.NomeAvaliacao = NomeAvaliacao;
            avaliacao.Inicio = AvaliacaoHash.Inicio;
            avaliacao.Fim = AvaliacaoHash.Fim;
            avaliacao.IdVaga = idVaga;

            return Ok(new
            {
                IdAvaliacao = avaliacao.IdAvaliacao,
                NomeAvaliacao = avaliacao.NomeAvaliacao,
                Inicio = avaliacao.Inicio,
                Fim = avaliacao.Fim,
                IdVaga = avaliacao.IdVaga
            });
        }

        [HttpPost]
        [Route("Desafio/AtualizaAvaliacao/{IdAvaliacao}/{AtualizacaoNomeAvaliacao}/{AtualizacaoIdVaga}")]
        public IHttpActionResult AtualizaAvaliacao(int IdAvaliacao, string AtualizacaoNomeAvaliacao, int AtualizacaoIdVaga, [FromBody] AvaliacaoHash AvaliacaoHash)
        {

            User usuario = Autenticar(AvaliacaoHash.Hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (AtualizacaoNomeAvaliacao == "" || AtualizacaoNomeAvaliacao == null || AvaliacaoHash.Inicio == null || AvaliacaoHash.Fim == null || AtualizacaoIdVaga == null)
                return Ok(new { message = "Dados errados." });

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbAvaliacao
                                            		   WHERE idAvaliacao = @IdAvaliacao)
                                            	BEGIN
                                            		UPDATE A
                                            		SET Avaliacao = @NomeAvaliacao
                                            		  , Inicio = @Inicio
                                            		  , Fim = @Fim
                                            		  , idVaga = @IdVaga
                                            		FROM tbAvaliacao A
                                            		WHERE A.idAvaliacao = @IdAvaliacao
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdAvaliacao = IdAvaliacao,
                    NomeAvaliacao = AtualizacaoNomeAvaliacao,
                    Inicio = AvaliacaoHash.Inicio,
                    Fim = AvaliacaoHash.Fim,
                    IdVaga = AtualizacaoIdVaga
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpDelete]
        [Route("Desafio/ApagarAvaliacao/{IdAvaliacao}")]
        public IHttpActionResult ApagarAvaliacao(int IdAvaliacao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbAvaliacao
                                            		   WHERE idAvaliacao = @IdAvaliacao)
                                            	BEGIN

													DELETE 
													FROM tbResposta
													WHERE idQuestao IN (SELECT idQuestao
                                            						    FROM tbQuestao 
                                            						    WHERE idAvaliacao = @IdAvaliacao) 

                                            		DELETE
                                            		FROM tbQuestao 
                                            		WHERE idAvaliacao = @IdAvaliacao
                                            		
                                            		DELETE
                                            		FROM tbAvaliacao
                                            		WHERE idAvaliacao = @IdAvaliacao
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdAvaliacao = IdAvaliacao
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("Desafio/Avaliacoes/{idVaga}")]
        public IHttpActionResult Avaliacoes(int idVaga,[FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Avaliacao> avaliacoes;

            using (var conn = new SqlConnection(ConnectionString))
            {
                avaliacoes = conn.Query<Avaliacao>(@"SELECT idAvaliacao IdAvaliacao
														  , Avaliacao NomeAvaliacao
														  , Inicio
														  , Fim
														  , idVaga IdVaga
                                                     FROM tbAvaliacao
                                                     WHERE idVaga = @idVaga", new { idVaga = idVaga }).ToList();
            }

            return Ok(avaliacoes.Select(a => new
            {
                IdAvaliacao = a.IdAvaliacao,
                NomeAvaliacao = a.NomeAvaliacao,
                Inicio = a.Inicio,
                Fim = a.Fim,
                IdVaga = a.IdVaga
            }));
        }

        [HttpPost]
        [Route("Desafio/NovaQuestao/{NomeQuestao}/{RespostaCorreta}/{idAvaliacao}")]
        public IHttpActionResult NovaQuestao(string NomeQuestao, string RespostaCorreta, int IdAvaliacao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (NomeQuestao == "" || NomeQuestao == null || RespostaCorreta == null || RespostaCorreta == "" || IdAvaliacao == 0)
                return Ok(new { message = "Dados errados." });

            Questao questao = new Questao();

            using (var conn = new SqlConnection(ConnectionString))
            {
                questao.IdQuestao= conn.Query<int>(@"IF EXISTS (SELECT *
                                                                FROM tbQuestao
                                                     	 	    WHERE Questao = @NomeQuestao
                                                                AND RespostaCorreta = @RespostaCorreta
												     		    AND idAvaliacao = @IdAvaliacao)
                                                         BEGIN
                                                     	     SELECT 0;
                                                         END
                                                     ELSE
                                                         BEGIN
                                                     	     INSERT INTO tbQuestao
												     		 VALUES (@NomeQuestao, @RespostaCorreta, @IdAvaliacao)
                                                             
												     		 SELECT idQuestao
                                                     		 FROM tbQuestao
                                                     	 	 WHERE Questao = @NomeQuestao
                                                             AND RespostaCorreta = @RespostaCorreta
												     		 AND idAvaliacao = @IdAvaliacao
												     	 END", new
                {
                    NomeQuestao = NomeQuestao,
                    RespostaCorreta = RespostaCorreta,
                    IdAvaliacao = IdAvaliacao
                }).FirstOrDefault();
            }

            if (questao.IdQuestao == 0)
                return Ok(new { message = "Questao ja existente." });

            questao.NomeQuestao = NomeQuestao;
            questao.RespostaCorreta = RespostaCorreta;
            questao.IdAvaliacao = IdAvaliacao;

            return Ok(new
            {
                IdQuestao = questao.IdQuestao,
                Questao = questao.NomeQuestao,
                RespostaCorreta = questao.RespostaCorreta,
                IdAvaliacao = questao.IdAvaliacao
            });
        }

        [HttpPost]
        [Route("Desafio/AtualizarQuestao/{IdQuestao}/{AtualizacaoNomeQuestao}/{AtualizacaoRespostaCorreta}/{AtualizacaoIdAvaliacao}")]
        public IHttpActionResult AtualizaQuestao(int IdQuestao, string AtualizacaoNomeQuestao, string AtualizacaoRespostaCorreta, int AtualizacaoIdAvaliacao, [FromBody] string hash)
        {

            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (AtualizacaoNomeQuestao == "" || AtualizacaoNomeQuestao == null || AtualizacaoRespostaCorreta == "" || AtualizacaoRespostaCorreta == null || AtualizacaoIdAvaliacao == 0)
                return Ok(new { message = "Dados errados." });

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbQuestao
                                            		   WHERE idQuestao = @IdQuestao)
                                            	BEGIN
                                            		UPDATE Q
                                            		SET Questao = @Questao
                                            		  , RespostaCorreta = @RespostaCorreta
                                            		  , idAvaliacao = @IdAvaliacao
                                            		FROM tbQuestao Q
                                            		WHERE Q.idQuestao = @IdQuestao
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdQuestao = IdQuestao,
                    Questao = AtualizacaoNomeQuestao,
                    RespostaCorreta = AtualizacaoRespostaCorreta,
                    IdAvaliacao = AtualizacaoIdAvaliacao
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpDelete]
        [Route("Desafio/ApagarQuestao/{IdQuestao}")]
        public IHttpActionResult ApagarQuestao(int IdQuestao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbQuestao
                                            		   WHERE idQuestao = @IdQuestao)
                                            	BEGIN
													DELETE 
													FROM tbResposta
													WHERE idQuestao = @IdQuestao

                                            		DELETE
                                            		FROM tbQuestao 
                                            		WHERE idQuestao = @IdQuestao
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdQuestao = IdQuestao
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("Desafio/Questoes/{idAvaliacao}")]
        public IHttpActionResult Questoes(int idAvaliacao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Questao> questoes;

            using (var conn = new SqlConnection(ConnectionString))
            {
                questoes = conn.Query<Questao>(@"SELECT idQuestao IdQuestao
                                                      , Questao NomeQuestao
                                                 	  , RespostaCorreta
                                                 	  , idAvaliacao IdAvaliacao
                                                 FROM tbQuestao
                                                 WHERE idAvaliacao = @idAvaliacao", new { idAvaliacao = idAvaliacao }).ToList();
            }

            return Ok(questoes.Select(q => new
            {
                IdQuestao = q.IdQuestao,
                Questao = q.NomeQuestao,
                RespostaCorreta = q.RespostaCorreta,
                IdAvaliacao = q.IdAvaliacao
            }));
        }

        [HttpPost]
        [Route("Desafio/NovaResposta/{NomeResposta}/{idQuestao}/{idUser}")]
        public IHttpActionResult NovaResposta(string NomeResposta, int idQuestao, int IdUser, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (NomeResposta == "" || NomeResposta == null || idQuestao == 0|| IdUser == 0)
                return Ok(new { message = "Dados errados." });

            Resposta resposta = new Resposta();

            using (var conn = new SqlConnection(ConnectionString))
            {
                resposta.IdResposta = conn.Query<int>(@"IF EXISTS (SELECT *
                                                        	 	   FROM tbResposta
                                                        	 	   WHERE Resposta = @Resposta
                                                                   AND idQuestao = @IdQuestao
												        		   AND idUser = @IdUser)
                                                            BEGIN
                                                        	    SELECT 0;
                                                            END
                                                        ELSE
                                                            BEGIN
                                                        	    INSERT INTO tbResposta
												        		VALUES (@Resposta, @IdQuestao, @IdUser)
                                                                
												        		SELECT idResposta
                                                        		FROM tbResposta
                                                        	 	WHERE Resposta = @Resposta
                                                                AND idQuestao = @IdQuestao
												        		AND idUser = @IdUser
												        	END", new
                {
                    Resposta = NomeResposta,
                    IdQuestao = idQuestao,
                    IdUser = IdUser
                }).FirstOrDefault();
            }

            if (resposta.IdResposta == 0)
                return Ok(new { message = "Resposta ja existente." });

            resposta.NomeResposta = NomeResposta;
            resposta.IdQuestao = idQuestao;
            resposta.IdUser = IdUser;

            return Ok(new
            {
                IdResposta = resposta.IdResposta,
                Resposta = resposta.NomeResposta,
                idQuestao = resposta.IdQuestao,
                idUser = resposta.IdUser
            });
        }

        [HttpPost]
        [Route("Desafio/AtualizarResposta/{idResposta}/{NomeRespostaAtualizada}/{idQuestaoAtualizada}/{idUserAtualizada}")]
        public IHttpActionResult AtualizarResposta(int IdResposta, string NomeRespostaAtualizada, int idQuestaoAtualizada, int idUserAtualizada, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            if (NomeRespostaAtualizada == "" || NomeRespostaAtualizada == null || idQuestaoAtualizada == 0 || idUserAtualizada == 0)
                return Ok(new { message = "Dados errados." });

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                                       FROM tbResposta
                                             		   WHERE idResposta = @IdResposta)
                                                BEGIN
                                            	    UPDATE R
                                            		SET Resposta = @Resposta
													  , idQuestao = @IdQuestao
													  , idUser = @IdUser
                                            		FROM tbResposta R
                                            		WHERE R.idResposta = @IdResposta
                                            
                                            		SELECT 1
                                                END
                                            ELSE
                                            	BEGIN
                                            	    SELECT 0
                                            	END", new
                {
                    IdResposta = IdResposta,
                    Resposta = NomeRespostaAtualizada,
                    IdQuestao = idQuestaoAtualizada,
                    IdUser = idUserAtualizada
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpDelete]
        [Route("Desafio/ApagarResposta/{IdResposta}")]
        public IHttpActionResult ApagarResposta(int IdResposta, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"IF EXISTS (SELECT *
                                            		   FROM tbResposta
                                            		   WHERE idResposta = @IdResposta)
                                            	BEGIN
													DELETE 
													FROM tbResposta
													WHERE idResposta = @IdResposta
                                            
                                            		SELECT 1
                                            	END
                                            ELSE
                                            	BEGIN
                                            		SELECT 0
                                            	END", new
                {
                    IdResposta = IdResposta
                }).FirstOrDefault();
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("Desafio/Resposta/{idQuestao}")]
        public IHttpActionResult Resposta(int idQuestao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Resposta> respostas;

            using (var conn = new SqlConnection(ConnectionString))
            {
                respostas = conn.Query<Resposta>(@"SELECT idResposta
                                                        , Resposta NomeResposta
                                                        , idQuestao
                                                  	    , idUser
                                                   FROM tbResposta
                                                   WHERE idQuestao = @idQuestao", new { idQuestao = idQuestao }).ToList();
            }

            return Ok(respostas.Select(r => new
            {
                idResposta = r.IdResposta,
                Resposta = r.NomeResposta,
                idQuestao = r.IdQuestao,
                idUser = r.IdUser
            }));
        }

        [HttpPost]
        [Route("Desafio/VagasDeCandidato/{idUser}")]
        public IHttpActionResult VagasDeCandidato(int idUser, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Vaga> vagas;

            string nome;

            using (var conn = new SqlConnection(ConnectionString))
            {
                vagas = conn.Query<Vaga>(@"SELECT V.Vaga NomeVaga
                                                , V.Descricao
                                           FROM tbUser U
                                           INNER JOIN tbResposta R
                                           ON U.idUser = R.idUser
                                           INNER JOIN tbQuestao Q
                                           ON R.idQuestao = Q.idQuestao
                                           INNER JOIN tbAvaliacao A
                                           ON Q.idAvaliacao = A.idAvaliacao
                                           INNER JOIN tbVaga V
                                           ON A.idVaga = V.idVaga
                                           WHERE U.idUser = @IdUser
                                           GROUP BY V.Vaga
                                                  , V.Descricao", new { IdUser = idUser }).ToList();

                nome = conn.Query<string>(@"SELECT Nome
                                            FROM tbUser
                                            WHERE idUser = @IdUser", new { IdUser = idUser }).FirstOrDefault();

            }

            return Ok(vagas.Select(v => new
            {
                idUser = idUser,
                Nome = nome,
                Vaga = v.NomeVaga,
                Descricao = v.Descricao
            }));
        }

        [HttpPost]
        [Route("Desafio/NotaCadidatoAvaliacao/{idUser}/{idAvaliacao}")]
        public IHttpActionResult NotaCadidatoAvaliacao(int idUser, int IdAvaliacao, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<Nota> notasAvaliacoes;

            string nome;

            using (var conn = new SqlConnection(ConnectionString))
            {
                notasAvaliacoes = conn.Query<Nota>(@"SELECT A.Avaliacao Nome
                                                          , R.Resposta
                                                          , Q.RespostaCorreta
                                                     FROM tbUser U
                                                     INNER JOIN tbResposta R
                                                     ON U.idUser = R.idUser
                                                     INNER JOIN tbQuestao Q
                                                     ON R.idQuestao = Q.idQuestao
                                                     INNER JOIN tbAvaliacao A
                                                     ON Q.idAvaliacao = A.idAvaliacao
                                                     WHERE U.idUser = @IdUser
                                                     AND A.idAvaliacao = @IdAvaliacao", new
                {
                    IdUser = idUser,
                    IdAvaliacao = IdAvaliacao
                }).ToList();

                nome = conn.Query<string>(@"SELECT Nome
                                            FROM tbUser
                                            WHERE idUser = @IdUser", new { IdUser = idUser }).FirstOrDefault();
            }

            int count = 0;

            for (int x = 0; x < notasAvaliacoes.Count; x++)
            {
                if (notasAvaliacoes[x].Resposta == notasAvaliacoes[x].RespostaCorreta)
                    count++;
            }

            decimal nota = (count * 10) / (notasAvaliacoes.Count);

            return Ok(new
            {
                idUser = idUser,
                Nome = nome,
                IdAvaliacao,
                Avaliacao = notasAvaliacoes.First().Nome,
                Nota = nota
            });
        }

        [HttpPost]
        [Route("Desafio/MelhorCandidatoVaga/{IdVaga}")]
        public IHttpActionResult MelhorCandidatoVaga(int IdVaga, [FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<User> candidatos;

            string nome;

            Dictionary<int, float> userNotas = new Dictionary<int, float>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                candidatos = conn.Query<User>(@"SELECT U.idUser
                                                     , U.Nome
                                                FROM tbUser U
                                                INNER JOIN tbResposta R
                                                ON U.idUser = R.idUser
                                                INNER JOIN tbQuestao Q
                                                ON R.idQuestao = Q.idQuestao
                                                INNER JOIN tbAvaliacao A
                                                ON Q.idAvaliacao = A.idAvaliacao
                                                INNER JOIN tbVaga V
                                                ON A.idVaga = V.idVaga
                                                WHERE V.idVaga = @IdVaga
                                                GROUP BY U.idUser
                                                       , U.Nome", new
                {
                    IdVaga = IdVaga
                }).ToList();

                nome = conn.Query<string>(@"SELECT Vaga
                                            FROM tbVaga
                                            WHERE idVaga = @IdVaga", new { IdVaga = IdVaga }).FirstOrDefault();

                for (int x = 0; x < candidatos.Count; x++)
                {
                    IList<Nota> notasVagas;

                    notasVagas = conn.Query<Nota>(@"SELECT R.Resposta
                                                         , Q.RespostaCorreta
                                                    FROM tbUser U
                                                    INNER JOIN tbResposta R
                                                    ON U.idUser = R.idUser
                                                    INNER JOIN tbQuestao Q
                                                    ON R.idQuestao = Q.idQuestao
                                                    INNER JOIN tbAvaliacao A
                                                    ON Q.idAvaliacao = A.idAvaliacao
													INNER JOIN tbVaga V
													ON A.idVaga = V.idVaga
                                                    WHERE U.idUser = @IdUser
                                                    AND V.idVaga = @IdVaga", new
                    {
                        IdUser = candidatos[x].IdUser,
                        IdVaga = IdVaga
                    }).ToList();

                    int count = 0;

                    for (int y = 0; y < notasVagas.Count; y++)
                    {
                        if (notasVagas[y].Resposta == notasVagas[y].RespostaCorreta)
                            count++;
                    }

                    userNotas.Add(candidatos[x].IdUser, ((count * 10) / notasVagas.Count));
                }
            }

            KeyValuePair<int, float> melhorCandidato = userNotas.OrderByDescending(u => u.Value).First();

            return Ok(new
            {
                IdUser = melhorCandidato.Key,
                Nome = candidatos.Where(c => c.IdUser == melhorCandidato.Key).Select(c => c.Nome).First(),
                IdVaga = IdVaga,
                Vaga = nome,
                Nota = melhorCandidato.Value
            });
        }

        [HttpPost]
        [Route("Desafio/VagasCandidatado")]
        public IHttpActionResult VagasCandidatado([FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            IList<Vaga> vagas;

            using (var conn = new SqlConnection(ConnectionString))
            {
                vagas = conn.Query<Vaga>(@"SELECT V.Vaga NomeVaga
                                                , V.Descricao
                                           FROM tbUser U
                                           INNER JOIN tbResposta R
                                           ON U.idUser = R.idUser
                                           INNER JOIN tbQuestao Q
                                           ON R.idQuestao = Q.idQuestao
                                           INNER JOIN tbAvaliacao A
                                           ON Q.idAvaliacao = A.idAvaliacao
										   INNER JOIN tbVaga V
										   ON A.idVaga = V.idVaga
                                           WHERE U.idUser = @IdUser
                                           GROUP BY V.Vaga
								                  , V.Descricao", new { IdUser = usuario.IdUser }).ToList();
            }

            return Ok(vagas.Select(v => new
            {
                IdUser = usuario.IdUser,
                Nome = usuario.Nome,
                Vaga = v.NomeVaga,
                Descricao = v.Descricao
            }));
        }

        [HttpPost]
        [Route("Desafio/NotasAvaliacoes")]
        public IHttpActionResult NotasAvaliacoes([FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            IList<Nota> notasAvaliacoes;

            Dictionary<int, float> avaliacaoNota = new Dictionary<int, float>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                notasAvaliacoes = conn.Query<Nota>(@"SELECT A.idAvaliacao Id
                                                     	  , A.Avaliacao Nome
                                                          , R.Resposta
                                                     	  , Q.RespostaCorreta
                                                     FROM tbUser U
                                                     INNER JOIN tbResposta R
                                                     ON U.idUser = R.idUser
                                                     INNER JOIN tbQuestao Q
                                                     ON R.idQuestao = Q.idQuestao
                                                     INNER JOIN tbAvaliacao A
                                                     ON Q.idAvaliacao = A.idAvaliacao
                                                     WHERE U.idUser = @IdUser", new { IdUser = usuario.IdUser }).ToList();
            }

            int[] avaliacoes = notasAvaliacoes.Select(n => n.Id).Distinct().ToArray();

            for (int x = 0; x < avaliacoes.Length; x++)
            {
                int count = 0;
                int questoes = 0;
                for (int y = 0; y < notasAvaliacoes.Count; y++)
                {
                    if(notasAvaliacoes[y].Id == avaliacoes[x])
                    {
                        questoes++;
                        if (notasAvaliacoes[y].Resposta == notasAvaliacoes[y].RespostaCorreta)
                            count++;
                    }
                }
                avaliacaoNota.Add(avaliacoes[x], ((count * 10) / questoes));
            }

            return Ok(avaliacaoNota.Select(a => new
            {
                IdUser = usuario.IdUser,
                Nome = usuario.Nome,
                IdAvaliacao = a.Key,
                Avaliacao = notasAvaliacoes.Where(w => w.Id == a.Key).Select(n => n.Nome).First(),
                Nota = a.Value
            }));
        }

        [HttpPost]
        [Route("Desafio/Candidatos")]
        public IHttpActionResult Candidatos([FromBody] string hash)
        {
            User usuario = Autenticar(hash);

            if (usuario == null)
                return Unauthorized();

            if (!Permissao(usuario))
                return Unauthorized();

            IList<User> candidatos;

            using (var conn = new SqlConnection(ConnectionString))
            {
                candidatos = conn.Query<User>(@"SELECT idUser IdUser
                                                	 , Nome
                                                FROM tbUser
                                                WHERE idCargo = 2").ToList();
            }

            return Ok(candidatos.Select(c => new
            {
                IdUser = c.IdUser,
                Nome = c.Nome
            }));
        }

        [HttpGet]
        [Route("Desafio/Login/{Nome}/{Senha}")]
        public IHttpActionResult VerifyLogin(string Nome, string Senha)
        {
            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"SELECT PWDCOMPARE(@Senha, Senha) 
                                            FROM tbUser 
                                            WHERE Nome = @Nome", new
                {
                    Senha = Senha,
                    Nome = Nome
                }).FirstOrDefault();
            }

            string hash = GenerateHash(Senha + "-" + Nome);

            if (!result)
                return Unauthorized();

            return Ok(new
            {
                Sucesso = result,
                Hash = hash
            });
        }

        private User Autenticar(string hash)
        {
            string senhaNome = Encoding.UTF32.GetString(Convert.FromBase64String(hash));

            string senha = senhaNome.Split('-')[0].Replace("\"", "");

            string nome = senhaNome.Split('-')[1].Replace("\"", "");

            User usuario = new User();

            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"SELECT PWDCOMPARE(@Senha, Senha) 
                                            FROM tbUser 
                                            WHERE Nome = @Nome", new
                {
                    Senha = senha,
                    Nome = nome
                }).FirstOrDefault();

                if(result)
                {
                    usuario = conn.Query<User>(@"SELECT idUser IdUser
                                                 	  , Nome
                                                 	  , idCargo IdCargo
                                                 FROM tbUser
                                                 WHERE Nome = @Nome
                                                 AND PWDCOMPARE(@Senha, Senha) = 1", new
                    {
                        Senha = senha,
                        Nome = nome
                    }).FirstOrDefault();
                    usuario.Hash = hash;
                }
            }

            if (result)
                return usuario;

            return null;
        }

        private string GenerateHash(string Senha)
        {
            return Convert.ToBase64String(Encoding.UTF32.GetBytes(Senha));
        }

        private bool Permissao(User usuario)
        {
            bool result = false;

            using (var conn = new SqlConnection(ConnectionString))
            {
                result = conn.Query<bool>(@"SELECT Permissao
                                            FROM tbCargo
                                            WHERE idCargo = @IdCargo", new
                {
                    IdCargo = usuario.IdCargo
                }).FirstOrDefault();
            }

            return result;
        }
    }
}
