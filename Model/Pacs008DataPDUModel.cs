using Pacs008.Request.Model;
using System.Xml.Serialization;

namespace Pacs008.Request.Model
{

    [XmlRoot(ElementName = "DataPDU", Namespace = "urn:swift:saa:xsd:saa.2.0")]
    public class DataPDUPacs008
    {
        [XmlElement(ElementName = "Header", Namespace = "urn:swift:saa:xsd:saa.2.0")]
        public Header? Header { get; set; }

        [XmlElement(ElementName = "Body", Namespace = "urn:swift:saa:xsd:saa.2.0")]
        public Body? Body { get; set; }
    }

    public class Header
    {
        [XmlElement(ElementName = "Message", Namespace = "urn:swift:saa:xsd:saa.2.0")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        public string? SenderReference { get; set; }
        public string? MessageIdentifier { get; set; }
        public string? Format { get; set; }
        public string? SubFormat { get; set; }
        public Party? Sender { get; set; }
        public Party? Receiver { get; set; }
        public InterfaceInfo? InterfaceInfo { get; set; }
        public NetworkInfo? NetworkInfo { get; set; }
    }

    public class Party
    {
        public string? DN { get; set; }
        public FullName? FullName { get; set; }
    }

    public class FullName
    {
        public string? X1 { get; set; }
    }

    public class InterfaceInfo
    {
        public string? UserReference { get; set; }
        public string? MessageCreator { get; set; }
    }

    public class NetworkInfo
    {
        public string? Priority { get; set; }
        public string? Service { get; set; }
        public string? Network { get; set; }
    }

}
