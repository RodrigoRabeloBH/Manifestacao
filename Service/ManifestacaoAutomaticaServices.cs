using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Data;
using Model;

namespace Service
{
    public class ManifestacaoAutomaticaServices
    {
        private readonly ManifestacaoRepository _rep = new ManifestacaoRepository();
        private readonly string dir_certificados = @"C:\NFE\certificados\";
        private readonly string url_webservice = @"https://www.nfe.fazenda.gov.br/NFeRecepcaoEvento4/NFeRecepcaoEvento4.asmx";

        public async Task ManifestaNotas()
        {
            IEnumerable<Informacao> informacoes = await _rep.GetInformacoes();

            foreach (Informacao info in informacoes)
            {
                try
                {
                    DateTime dataManifestacao = DateTime.Now;

                    RecepcaoEvento consulta = new RecepcaoEvento(info.Versao, info.Chave, info.CodigoOrgao,
                                                                 info.TipoAmbiente, info.Cnpj,
                                                                 dataManifestacao.ToString("yyyy-MM-ddTHH:mm:ss") + "-03:00",
                                                                 info.CodigoEvento, info.NumSeqEvento, info.VersaoEvento, info.Descricao,
                                                                 "", "");
                    try
                    {
                        Resultado resultado = RequesteRecepcaoEvento(url_webservice, "", @dir_certificados + info.PathCertificado, info.Senha, consulta);
                    }
                    catch (Exception ex)
                    {

                        if (ex.Message == "Rejeicao: A data do evento nao pode ser menor que a data de emissao da NF-e" ||
                            ex.Message == "Rejeicao: A data do evento nao pode ser menor que a data de autorizacao para NF-e nao emitida em contingencia")
                        {
                            consulta.DhEvento = dataManifestacao.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss") + "-03:00";

                            Resultado result = RequesteRecepcaoEvento(url_webservice, null, @dir_certificados + info.PathCertificado, info.Senha, consulta);

                        }
                        else if (ex.Message == "Rejeicao: A data do evento nao pode ser maior que a data do processamento")
                        {
                            consulta.DhEvento = dataManifestacao.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ss") + "-03:00";

                            Resultado result = RequesteRecepcaoEvento(url_webservice, null, @dir_certificados + info.PathCertificado, info.Senha, consulta);

                        }
                        else
                        {
                            throw new Exception(ex.Message);
                        }
                    }

                    await _rep.ConfirmaManifestacao(1, info.Chave);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Rejeicao: Duplicidade de evento")
                    {
                        await _rep.ConfirmaManifestacao(1, info.Chave);
                    }
                    else
                    {
                        throw new Exception("Error ao Manifestar DFe \n" + ex.Message);
                    }
                }
                Console.WriteLine($"\n \t \t CNPJ: {info.Cnpj} -- Chave: {info.Chave}  \n");
            }
        }

        private Resultado RequesteRecepcaoEvento(string url_webservice, string xJust, string certificado_path, string password, RecepcaoEvento consulta)
        {
            string msg_padrao = "<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"" + consulta.Versao + "\"><idLote>1</idLote><evento versao=\"" + consulta.Versao + "\"><infEvento Id=\"ID" + consulta.TpEvento + consulta.ChNFE + "01\"><cOrgao>" + consulta.COrgao + "</cOrgao><tpAmb>" + consulta.TpAmb + "</tpAmb><CNPJ>" + consulta.CNPJ + "</CNPJ><chNFe>" + consulta.ChNFE + "</chNFe><dhEvento>" + consulta.DhEvento + "</dhEvento><tpEvento>" + consulta.TpEvento + "</tpEvento><nSeqEvento>" + consulta.NSeqEvento + "</nSeqEvento><verEvento>" + consulta.VerEvento + "</verEvento><detEvento versao=\"" + consulta.VerEvento + "\"><descEvento>" + consulta.DescEvento + "</descEvento>" + ((!String.IsNullOrWhiteSpace(xJust)) ? "<xJust>" + xJust + "</xJust>" : "") + "</detEvento></infEvento></evento></envEvento>";

            XmlDocument xmlAss = new XmlDocument();

            xmlAss.LoadXml(AssinaXML(msg_padrao, consulta.ChNFE, consulta.TpEvento, certificado_path, password).OuterXml);

            string msg_soap = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:nfer=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4\"><soapenv:Header/><soapenv:Body><nfer:nfeDadosMsg>" + xmlAss.InnerXml + "</nfer:nfeDadosMsg></soapenv:Body></soapenv:Envelope>";

            XmlDocument Resposta_WS = new XmlDocument();

            string result_request = "";

            Resultado retorno;

            try
            {
                result_request = SOAP_WEBREQUEST(url_webservice, msg_soap, certificado_path, password);
            }
            catch (WebException)
            {
                throw new WebException("Erro na conexão com o webservice");
            }
            try
            {
                Resposta_WS.LoadXml(result_request);

                retorno.verAplic = Resposta_WS.GetElementsByTagName("verAplic").Item(1).FirstChild.Value;

                retorno.cStat = Resposta_WS.GetElementsByTagName("cStat").Item(1).FirstChild.Value;

                retorno.xMotivo = Resposta_WS.GetElementsByTagName("xMotivo").Item(1).FirstChild.Value;

                retorno.cOrgao = Resposta_WS.GetElementsByTagName("cOrgao").Item(1).FirstChild.Value;

                retorno.tpEvento = Resposta_WS.GetElementsByTagName("tpEvento").Item(0).FirstChild.Value;

                retorno.chNFe = Resposta_WS.GetElementsByTagName("chNFe").Item(0).FirstChild.Value;
            }
            catch
            {
                throw new Exception("Erro nos dados de retorno da SEFAZ");
            }
            if (retorno.cStat != "135")
            {
                throw new Exception(retorno.xMotivo);
            }

            return (retorno);
        }

        private string SOAP_WEBREQUEST(string url_webservice, string msg, string certificado_path, string password)
        {
            HttpClient client = CreateWebRequest(@url_webservice, @certificado_path, password);

            XmlDocument soapEnvelopeXml = new XmlDocument();

            soapEnvelopeXml.LoadXml(msg);

            var content = new StringContent(soapEnvelopeXml.InnerXml.ToString(), Encoding.UTF8, "text/xml");

            var response = client.PostAsync(url_webservice, content).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            return result;
        }

        private HttpClient CreateWebRequest(string url, string certificatePath, string password)
        {
            var handler = new HttpClientHandler();

            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

            handler.ClientCertificateOptions = ClientCertificateOption.Manual;

            handler.SslProtocols = SslProtocols.Tls12;

            X509Certificate Cert = new X509Certificate2(certificatePath, password);

            handler.ClientCertificates.Add(Cert);

            return new HttpClient(handler);
        }

        private XmlDocument AssinaXML(string xml_to_sign, string chNFE, string tpEvento, string certificado_path, string password)
        {

            XmlDocument xml = new XmlDocument();

            xml.LoadXml(@xml_to_sign);

            //Classe usada para assinar o Documento XML
            SignedXml signedXml = new SignedXml(xml);

            //Configura a chave de assinatura
            X509Certificate2 Cert = new X509Certificate2(@certificado_path, password);

            //Configura chave de Assintura
            signedXml.SigningKey = Cert.PrivateKey;

            //Create Reference
            Reference reference = new Reference();
            reference.Uri = "#ID" + tpEvento + chNFE + "01";

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();

            reference.AddTransform(env);

            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();

            reference.AddTransform(c14);

            reference.DigestMethod = SignedXml.XmlDsigSHA1Url;

            signedXml.AddReference(reference);

            // Create a reference to be signed.
            KeyInfo keyInfo = new KeyInfo();

            keyInfo.AddClause(new KeyInfoX509Data(Cert));

            signedXml.KeyInfo = keyInfo;

            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

            signedXml.ComputeSignature();

            XmlElement xmlDigitalSignature = signedXml.GetXml();

            xml.GetElementsByTagName("evento").Item(0).AppendChild(xml.ImportNode(xmlDigitalSignature, true));

            return xml;
        }
    }
}