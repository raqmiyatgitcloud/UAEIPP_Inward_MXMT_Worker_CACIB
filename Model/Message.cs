//using System.Xml.Serialization;

//namespace Pacs008.Request.Model
//{
//    [XmlRoot("Messages")]
//    public class Messages
//    {
//        [XmlElement(ElementName = "MX")]
//        public MX? MX { get; set; }
//    }
//    [XmlRoot("MX")]
//    public class MX
//    {
//        [XmlElement("DataPDU", Namespace = "urn:swift:saa:xsd:saa.2.0")]
//        public DataPDU? DataPDU { get; set; }
//    }
//    [XmlRoot("DataPDU")]
//    public class DataPDU
//    {
//        [XmlElement(ElementName = "Revision")]
//        public string? Revision { get; set; }
//        [XmlElement(ElementName = "Header")]
//        public Header? Header { get; set; }
//    }
//    [XmlRoot(ElementName = "Header")]
//    public class Header
//    {
//        [XmlElement(ElementName = "Message")]
//        public Message? Message { get; set; }
//    }
//    [XmlRoot(ElementName = "Message")]
//    public class Message
//    {
//        [XmlElement(ElementName = "SenderReference")]
//        public string? SenderReference { get; set; }

//        [XmlElement(ElementName = "MessageIdentifier")]
//        public string? MessageIdentifier { get; set; }

//        [XmlElement(ElementName = "Format")]
//        public string? Format { get; set; }

//        [XmlElement(ElementName = "SubFormat")]
//        public string? SubFormat { get; set; }

//        [XmlElement(ElementName = "Sender")]
//        public Sender? Sender { get; set; }

//        [XmlElement(ElementName = "Receiver")]
//        public Receiver? Receiver { get; set; }

//        [XmlElement(ElementName = "InterfaceInfo")]
//        public InterfaceInfo? InterfaceInfo { get; set; }

//        [XmlElement(ElementName = "NetworkInfo")]
//        public NetworkInfo? NetworkInfo { get; set; }

//        [XmlElement(ElementName = "SecurityInfo")]
//        public SecurityInfo? SecurityInfo { get; set; }
//    }
//    [XmlRoot(ElementName = "Sender")]
//    public class Sender
//    {
//        [XmlElement(ElementName = "DN")]
//        public string? DN { get; set; }

//        [XmlElement(ElementName = "FullName")]
//        public FullName? FullName { get; set; }
//    }
//    [XmlRoot(ElementName = "Receiver")]
//    public class Receiver
//    {
//        [XmlElement(ElementName = "DN")]
//        public string? DN { get; set; }

//        [XmlElement(ElementName = "FullName")]
//        public FullName? FullName { get; set; }
//    }
//    [XmlRoot(ElementName = "FullName")]
//    public class FullName
//    {
//        [XmlElement(ElementName = "X1")]
//        public string? X1 { get; set; }
//    }
//    [XmlRoot(ElementName = "InterfaceInfo")]
//    public class InterfaceInfo
//    {
//        [XmlElement(ElementName = "UserReference")]
//        public string? UserReference { get; set; }
//        [XmlElement(ElementName = "MessageCreator")]
//        public string? MessageCreator { get; set; }
//        [XmlElement(ElementName = "MessageContext")]
//        public string? MessageContext { get; set; }
//        [XmlElement(ElementName = "MessageNature")]
//        public string? MessageNature { get; set; }
//        [XmlElement(ElementName = "Sumid")]
//        public string? Sumid { get; set; }

//    }
//    [XmlRoot(ElementName = "NetworkInfo")]
//    public class NetworkInfo
//    {
//        [XmlElement(ElementName = "Priority")]
//        public string? Priority { get; set; }
    
//        [XmlElement(ElementName = "Service")]
//        public string? Service { get; set; }


//        [XmlElement(ElementName = "Network")]
//        public string? Network { get; set; }

//        [XmlElement(ElementName = "SWIFTNetNetworkInfo")]
//        public SWIFTNetNetworkInfo? SWIFTNetNetworkInfo { get; set; }
//    }
//    [XmlRoot(ElementName = "SWIFTNetNetworkInfo")]
//    public class SWIFTNetNetworkInfo
//    {
//        [XmlElement(ElementName = "RequestType")]
//        public string? RequestType { get; set; }
//        [XmlElement(ElementName = "Reference")]
//        public string? Reference { get; set; }
      
//    }
//    [XmlRoot(ElementName = "SecurityInfo")]
//    public class SecurityInfo
//    {
//        [XmlElement(ElementName = "IsSigningRequested")]
//        public bool IsSigningRequested { get; set; }
//        [XmlElement(ElementName = "SWIFTNetSecurityInfo")]
//        public SWIFTNetSecurityInfo? SWIFTNetSecurityInfo { get; set; }
       
//    }
//    [XmlRoot(ElementName = "SWIFTNetSecurityInfo")]
//    public class SWIFTNetSecurityInfo
//    {
//        [XmlElement(ElementName = "IsNRRequested")]
//        public bool IsNRRequested { get; set; }
//    }
//}