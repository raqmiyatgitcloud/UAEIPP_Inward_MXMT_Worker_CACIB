using System.Xml;
using System.Xml.Serialization;

namespace Pacs008.Request.Model
{

	[XmlRoot(ElementName = "TtlIntrBkSttlmAmt")]
    public class TtlIntrBkSttlmAmt
    {

        [XmlAttribute(AttributeName = "Ccy")]
        public string? Ccy { get; set; }

        [XmlText]
        public string? Text { get; set; }
    }

    [XmlRoot(ElementName = "ClrSys")]
    public class ClrSys
    {

        [XmlElement(ElementName = "Prtry")]
        public string? Prtry { get; set; }
    }

    [XmlRoot(ElementName = "SttlmInf")]
    public class SttlmInf
    {

        [XmlElement(ElementName = "SttlmMtd")]
        public string? SttlmMtd { get; set; }

        [XmlElement(ElementName = "SttlmAcct")]
        public SttlmAcct? SttlmAcct { get; set; }

        [XmlElement(ElementName = "InstgRmbrsmntAgt")]
        public InstgRmbrsmntAgt? InstgRmbrsmntAgt { get; set; }

        [XmlElement(ElementName = "InstdRmbrsmntAgt")]
        public InstdRmbrsmntAgt? InstdRmbrsmntAgt { get; set; }
    }
    [XmlRoot(ElementName = "SttlmAcct")]
    public class SttlmAcct
    {

        [XmlElement(ElementName = "Id")]
        public Id? Id { get; set; }
    }
    [XmlRoot(ElementName = "FinInstnId")]
    public class FinInstnId
    {

        [XmlElement(ElementName = "BICFI")]
        public string? BICFI { get; set; }
    }

    [XmlRoot(ElementName = "InstgRmbrsmntAgt")]
    public class InstgRmbrsmntAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "InstdRmbrsmntAgt")]
    public class InstdRmbrsmntAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "GrpHdr")]
    public class GrpHdr
    {

        [XmlElement(ElementName = "MsgId")]
        public string? MsgId { get; set; }

        [XmlElement(ElementName = "CreDtTm")]
        public string? CreDtTm { get; set; }

        [XmlElement(ElementName = "NbOfTxs")]
        public string? NbOfTxs { get; set; }

        [XmlElement(ElementName = "TtlIntrBkSttlmAmt")]
        public TtlIntrBkSttlmAmt? TtlIntrBkSttlmAmt { get; set; }

        [XmlElement(ElementName = "IntrBkSttlmDt")]
        public string? IntrBkSttlmDt { get; set; }

        [XmlElement(ElementName = "SttlmInf")]
        public SttlmInf? SttlmInf { get; set; }

    }

    [XmlRoot(ElementName = "PmtId")]
    public class PmtId
    {

        [XmlElement(ElementName = "InstrId")]
        public string? InstrId { get; set; }

        [XmlElement(ElementName = "EndToEndId")]
        public string? EndToEndId { get; set; }

        [XmlElement(ElementName = "TxId")]
        public string? TxId { get; set; }

        [XmlElement(ElementName = "UETR")]
        public string? UETR { get; set; }
    }

    [XmlRoot(ElementName = "LclInstrm")]
    public class LclInstrm
    {

        [XmlElement(ElementName = "Prtry")]
        public string? Prtry { get; set; }
    }

    

    [XmlRoot(ElementName = "PmtTpInf")]
    public class PmtTpInf
    {

        [XmlElement(ElementName = "LclInstrm")]
        public LclInstrm? LclInstrm { get; set; }

        //[XmlElement(ElementName = "CtgyPurp")]
        //public CtgyPurp? CtgyPurp { get; set; }
    }

    [XmlRoot(ElementName = "IntrBkSttlmAmt")]
    public class IntrBkSttlmAmt
    {

        [XmlAttribute(AttributeName = "Ccy")]
        public string? Ccy { get; set; }

        [XmlText]
        public string? Text { get; set; }
    }

    [XmlRoot(ElementName = "SchmeNm")]
    public class SchmeNm
    {

        [XmlElement(ElementName = "Cd")]
        public string? Cd { get; set; }
    }
    [XmlRoot(ElementName = "DtAndPlcOfBirth")]
    public class DtAndPlcOfBirth
    {
        [XmlElement(ElementName = "BirthDt")]
        public string? BirthDt { get; set; }
        [XmlElement(ElementName = "CityOfBirth")]
        public string? CityOfBirth { get; set; }
        [XmlElement(ElementName = "CtryOfBirth")]
        public string? CtryOfBirth { get; set; }
    }


    [XmlRoot(ElementName = "Othr")]
    public class Othr
    {

        [XmlElement(ElementName = "Id")]
        public string? Id { get; set; }

        [XmlElement(ElementName = "SchmeNm")]
        public SchmeNm? SchmeNm { get; set; }

        [XmlElement(ElementName = "Issr")]
        public string? Issr { get; set; }
    }

    [XmlRoot(ElementName = "OrgId")]
    public class OrgId
    {

        [XmlElement(ElementName = "Othr")]
        public Othr? Othr { get; set; }
    }
    [XmlRoot(ElementName = "PrvtId")]
    public class PrvtId
    {
        [XmlElement(ElementName = "DtAndPlcOfBirth")]
        public DtAndPlcOfBirth? DtAndPlcOfBirth { get; set; }

        [XmlElement(ElementName = "Othr")]
        public Othr? Othr { get; set; }
    }

    [XmlRoot(ElementName = "Id")]
    public class Id
    {

        [XmlElement(ElementName = "OrgId")]
        public OrgId? OrgId { get; set; }
        [XmlElement(ElementName = "PrvtId")]
        public PrvtId? PrvtId { get; set; }

        [XmlElement(ElementName = "IBAN")]
        public string? IBAN { get; set; }

        [XmlElement(ElementName = "Othr")]
        public Othr? Othr { get; set; }

    }

    [XmlRoot(ElementName = "Dbtr")]
    public class Dbtr
    {

        [XmlElement(ElementName = "Nm")]
        public string? Nm { get; set; }

        [XmlElement(ElementName = "Id")]
        public Id? Id { get; set; }

        [XmlElement(ElementName = "CtctDtls")]
        public CtctDtls? CtctDtls { get; set; }
    }

    [XmlRoot(ElementName = "Tp")]
    public class Tp
    {

        [XmlElement(ElementName = "Cd")]
        public string? Cd { get; set; }
    }

    [XmlRoot(ElementName = "DbtrAcct")]
    public class DbtrAcct
    {

        [XmlElement(ElementName = "Id")]
        public Id? Id { get; set; }


        [XmlElement(ElementName = "Tp")]
        public Tp? Tp { get; set; }


    }

    [XmlRoot(ElementName = "CtctDtls")]
    public class CtctDtls
    {

        [XmlElement(ElementName = "NmPrfx")]
        public string? NmPrfx { get; set; }
    }

    public enum NamePrefix2Code
    {

        /// <remarks/>
        DOCT,

        /// <remarks/>
        MADM,

        /// <remarks/>
        MISS,

        /// <remarks/>
        MIST,

        /// <remarks/>
        MIKS,
    }

   public enum PreferredContactMethod1Code
    {

        /// <remarks/>
        LETT,

        /// <remarks/>
        MAIL,

        /// <remarks/>
        PHON,

        /// <remarks/>
        FAXX,

        /// <remarks/>
        CELL,
    }

    [XmlRoot(ElementName = "InstgAgt")]
    public class InstgAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "InstdAgt")]
    public class InstdAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }
    [XmlRoot(ElementName = "DbtrAgt")]
    public class DbtrAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "CdtrAgt")]
    public class CdtrAgt
    {

        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }

    [XmlRoot(ElementName = "Cdtr")]
    public class Cdtr
    {

        [XmlElement(ElementName = "Nm")]
        public string? Nm { get; set; }
    }

    [XmlRoot(ElementName = "CdtrAcct")]
    public class CdtrAcct
    {

        [XmlElement(ElementName = "Id")]
        public Id? Id { get; set; }


        [XmlElement(ElementName = "Tp")]
        public Tp? Tp { get; set; }
    }

    [XmlRoot(ElementName = "Purp")]
    public class Purp
    {

        [XmlElement(ElementName = "Prtry")]
        public string? Prtry { get; set; }
        [XmlElement(ElementName = "Cd")]
        public string? Cd { get; set; }
    }
    [XmlRoot(ElementName = "RmtInf")]
    public class RmtInf
    {
        [XmlElement("Ustrd")]
        public string? Ustrd { get; set; }
    }

    [XmlRoot(ElementName = "CdtTrfTxInf")]
    public class CdtTrfTxInf
    {

        [XmlElement(ElementName = "PmtId")]
        public PmtId? PmtId { get; set; }

        [XmlElement(ElementName = "PmtTpInf")]
        public PmtTpInf? PmtTpInf { get; set; }

        [XmlElement(ElementName = "IntrBkSttlmAmt")]
        public IntrBkSttlmAmt? IntrBkSttlmAmt { get; set; }

        [XmlElement(ElementName = "IntrBkSttlmDt")]
        public string? IntrBkSttlmDt { get; set; }

        [XmlElement(ElementName = "ChrgBr")]
        public string? ChrgBr { get; set; }

        [XmlElement(ElementName = "InstgAgt")]
        public InstgAgt? InstgAgt { get; set; }

        [XmlElement(ElementName = "InstdAgt")]
        public InstdAgt? InstdAgt { get; set; }

        [XmlElement(ElementName = "Dbtr")]
        public Dbtr? Dbtr { get; set; }

        [XmlElement(ElementName = "DbtrAcct")]
        public DbtrAcct? DbtrAcct { get; set; }

        [XmlElement(ElementName = "DbtrAgt")]
        public DbtrAgt? DbtrAgt { get; set; }

        [XmlElement(ElementName = "CdtrAgt")]
        public CdtrAgt? CdtrAgt { get; set; }

        [XmlElement(ElementName = "Cdtr")]
        public Cdtr? Cdtr { get; set; }

        [XmlElement(ElementName = "CdtrAcct")]
        public CdtrAcct? CdtrAcct { get; set; }

        [XmlElement(ElementName = "Purp")]
        public Purp? Purp { get; set; }

        [XmlElement("RmtInf")]
        public RmtInf? RmtInf { get; set; }
    }

    [XmlRoot(ElementName = "FIToFICstmrCdtTrf")]
    public class FIToFICstmrCdtTrf
    {

        [XmlElement(ElementName = "GrpHdr")]
        public GrpHdr? GrpHdr { get; set; }

        [XmlElement(ElementName = "CdtTrfTxInf")]
        public List<CdtTrfTxInf>? CdtTrfTxInf { get; set; }
    }
    [XmlRoot(ElementName = "Body")]
    public class Body
    {
        [XmlElement(ElementName = "AppHdr", Namespace = "urn:iso:std:iso:20022:tech:xsd:head.001.001.02")]
        public AppHdr? AppHdr { get; set; }

        [XmlElement(ElementName = "Document", Namespace = "urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08")]
        public Document? Document { get; set; }
    }

    [XmlRoot(ElementName = "AppHdr", Namespace = "urn:iso:std:iso:20022:tech:xsd:head.001.001.02")]
    public class AppHdr
    {
        [XmlElement(ElementName = "Fr")]
        public Fr? Fr { get; set; }

        [XmlElement(ElementName = "To")]
        public To? To { get; set; }

        [XmlElement(ElementName = "BizMsgIdr")]
        public string? BizMsgIdr { get; set; }

        [XmlElement(ElementName = "MsgDefIdr")]
        public string? MsgDefIdr { get; set; }

        [XmlElement(ElementName = "BizSvc")]
        public string? BizSvc { get; set; }

        [XmlElement(ElementName = "CreDt")]
        public string? CreDt { get; set; }
    }

    public class Fr
    {
        [XmlElement(ElementName = "FIId")]
        public FIId? FIId { get; set; }
    }

    public class To
    {
        [XmlElement(ElementName = "FIId")]
        public FIId? FIId { get; set; }
    }

    public class FIId
    {
        [XmlElement(ElementName = "FinInstnId")]
        public FinInstnId? FinInstnId { get; set; }
    }


    [XmlRoot(ElementName = "Document", Namespace = "urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08")]

	public class Document
    {

        [XmlElement(ElementName = "FIToFICstmrCdtTrf")]
        public FIToFICstmrCdtTrf? FIToFICstmrCdtTrf { get; set; }

        public string? Xmlns { get; set; }

        public string? Xsi { get; set; }

        public string? SchemaLocation { get; set; }
        [XmlText]
        public string? Text { get; set; }
    }
}


