using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.Oracle;
using Model;
using Oracle.ManagedDataAccess.Client;

namespace Data
{
    public class ManifestacaoRepository
    {
        private readonly string connectionString = "User Id=bsnotas;Password=bsnotas;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1550))(CONNECT_DATA=(SERVICE_NAME=BRUNSKER)))";
        public async Task<IEnumerable<Informacao>> GetInformacoes()
        {
            try
            {
                string sql = "pkg_clientes_nfe.proc_manifestacao_automatica";

                using (var conn = new OracleConnection(connectionString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    OracleDynamicParameters parms = new OracleDynamicParameters();

                    parms.Add("CUR_OUT", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

                    return await conn.QueryAsync<Informacao>(sql, parms, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task ConfirmaManifestacao(int seq_manifestacao, string chave)
        {
            try
            {
                string sql = "pkg_clientes_nfe.proc_confirma_manifestacao";

                using (var conn = new OracleConnection(connectionString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    OracleDynamicParameters parms = new OracleDynamicParameters();

                    parms.Add("pSEQ_MANIFESTACAO", seq_manifestacao);
                    parms.Add("pCHAVE", chave);

                    await conn.ExecuteAsync(sql, parms, commandType: CommandType.StoredProcedure);
                }
            }
            catch (OracleException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}