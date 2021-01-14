namespace Model
{
    public class RecepcaoEvento
    {
        public string Versao { get; set; }
        public string ChNFE { get; set; }
        public string COrgao { get; set; }
        public string TpAmb { get; set; }
        public string CNPJ { get; set; }
        public string DhEvento { get; set; }
        public string TpEvento { get; set; }
        public string NSeqEvento { get; set; }
        public string VerEvento { get; set; }
        public string DescEvento { get; set; }
        public string Codibge { get; set; }
        public string ObservacoesTomador { get; set; }
        public RecepcaoEvento(string versao, string chNFE, string cOrgao, string tpAmb,
                             string cNPJ, string dhEvento, string tpEvento, string nSeqEvento,
                             string verEvento, string descEvento, string codibge, string observacoesTomador)
        {
            Versao = versao;
            ChNFE = chNFE;
            COrgao = cOrgao;
            TpAmb = tpAmb;
            CNPJ = cNPJ;
            DhEvento = dhEvento;
            TpEvento = tpEvento;
            NSeqEvento = nSeqEvento;
            VerEvento = verEvento;
            DescEvento = descEvento;
            Codibge = codibge;
            ObservacoesTomador = observacoesTomador;
        }
    }
}
