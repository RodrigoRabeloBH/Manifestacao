using System;

namespace Model
{
    public class Informacao
    {
        public int CodManifestacao { get; set; }
        public string Chave { get; set; }
        public string Cnpj { get; set; }
        public DateTime DataEmissao { get; set; }
        public int Numero { get; set; }
        public string PathCertificado { get; set; }
        public string Senha { get; set; }
        public string CodigoEvento { get; set; }
        public string Descricao { get; set; }
        public string Versao { get; set; }
        public string CodigoOrgao { get; set; }
        public string TipoAmbiente { get; set; }
        public string VersaoEvento { get; set; }
        public string NumSeqEvento { get; set; }
        public string CodigoIbge { get; set; }
        public string Observacao { get; set; }
    }
}