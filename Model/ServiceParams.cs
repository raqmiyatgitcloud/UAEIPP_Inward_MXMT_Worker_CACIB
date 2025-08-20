namespace Raqmiyat.Framework.Model
{
	public class ServiceParams
	{
        public int CommandTimeout { get; set; }
        public int TimeoutMilliSeconds { get; set; }
        public string? EncryptDecryptKey { get; set; }
        public bool MtorMxCoreBank { get; set; }
        public string? Format { get; set; }
        public string ? PacsMsg { get; set; }
        public string ? SttlmAcct { get; set; }
        public bool IsDataPDU { get; set; }    
        public string? SttlmMtd { get; set; }    
        public string? SubFormat { get; set; }    
        public string? MessageCreator { get; set; }
        public string? Priority { get; set; }
        public string? Service { get; set; }
        public string? Network { get; set; }
        public bool IsXMLValidation { get; set; }
        public string? RmtInf { get; set; }


    }
}