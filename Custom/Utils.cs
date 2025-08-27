using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Raqmiyat.Framework.Custom
{
    public class Utils
    {
        private IDbConnection _idbConnection;
        private readonly ConnectCustom _connectCustom;
        public Utils(IDbConnection idbConnection, ConnectCustom connectCustom)
        {
            _idbConnection = idbConnection;
            _connectCustom = connectCustom;
        }
        //public async Task<string> Serialize<T>(T? objectToSerialize, Logger _logger)
        //{
        //    string serializedObj = string.Empty;
        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            using MemoryStream stream = new();
        //            XmlSerializerNamespaces ns = new();
        //            ns.Add("", "");
        //            var serializer = new XmlSerializer(typeof(T));
        //            var writer = new XmlTextWriter(stream, Encoding.UTF8);
        //            writer.WriteStartDocument(true);
        //            serializer.Serialize(writer, objectToSerialize, ns);
        //            serializedObj = Encoding.UTF8.GetString(stream.ToArray());
        //            serializedObj = serializedObj.Replace("utf-8", "UTF-8");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error("Utils", "Serialize", $"Error occurred in SerializeXml(): {ex.Message}");
        //        }
        //    });
        //    return serializedObj;
        //}


        public async Task<string> SerializeToXML<T>(T objectToSerialize, Logger _logger)
        {
            string result = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

                    using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
                    {
                        if (xmlWriter != null)
                        {
                            //xmlWriter.WriteStartDocument(false);
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add("", "");

                            new XmlSerializer(typeof(T)).Serialize(xmlWriter, objectToSerialize, ns);
                        }
                        xmlWriter!.Close();
                    }
                    result = sb.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Pacs003XmlWorker", "SerializeToXML", ex.Message);
                }
            });

            return result;
        }
        public async Task<string> SerializeToDataPDUXML<T>(T objectToSerialize, Logger _logger)
        {
            string result = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = new UTF8Encoding(false), // UTF-8 without BOM
                        OmitXmlDeclaration = false
                    };

                    T cleanedModel = RemoveNullOrEmptyProperties(objectToSerialize, _logger);

                    using (var ms = new MemoryStream())
                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, settings))
                    {
                        if (xmlWriter != null)
                        {
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add("Saa", "urn:swift:saa:xsd:saa.2.0");
                            ns.Add("Sw", "urn:swift:snl:ns.Sw");
                            ns.Add("SwSec", "urn:swift:snl:ns.SwSec");
                            ns.Add("SwInt", "urn:swift:snl:ns.SwInt");
                            ns.Add("SwGbl", "urn:swift:snl:ns.SwGbl");

                            new XmlSerializer(typeof(T)).Serialize(xmlWriter, objectToSerialize, ns);
                        }
                        xmlWriter!.Close();

                        // Convert MemoryStream (UTF-8 bytes) to string
                        result = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Pacs003XmlWorker", "SerializeToXML", ex.Message);
                }
            });

            return result;
        }

        public async Task<string> SerializeStringToXML(string value, Logger _logger)
        {
            string result = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

                    using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
                    {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("Root"); // custom root
                        xmlWriter.WriteElementString("Value", value); // your string inside element
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                    }

                    result = sb.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Pacs003XmlWorker", "SerializeStringToXML", ex.Message);
                }
            });

            return result;
        }


        public async Task<T> Deserialize<T>(string xmlContent, Logger _logger) where T : class
        {
            T? deserializedObject = null;
            await Task.Run(() =>
            {
                try
                {
                    XmlSerializer serializer = new(typeof(T));
                    using StringReader stringReader = new(xmlContent);
                    deserializedObject = serializer.Deserialize(stringReader) as T;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "Deserialize", $"Error during deserialization: {ex.Message}");
                }
            });
            return deserializedObject!;
        }
        public Task<bool> CheckValidXml(string message, Logger _logger)
        {
            bool xdocloaded = false;
            try
            {
                var xResDoc = new XmlDocument();
                xResDoc.LoadXml(message);
                xdocloaded = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Utils", "CheckValidXml", $"CheckValidXml - XmlDocument Loaded Exception: {ex.Message}");
            }
            return Task.FromResult(xdocloaded);
        }
        public async Task<bool> IsValidJson(string strInput, Logger _logger)
        {
            bool result = false;
            await Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(strInput))
                {

                    strInput = strInput.Trim();
                    if ((strInput.StartsWith('{') && strInput.EndsWith('}')) || //For object
                        (strInput.StartsWith('[') && strInput.EndsWith(']'))) //For array
                    {
                        try
                        {
                            JToken.Parse(strInput);
                            result = true;
                        }
                        catch (JsonReaderException jex)
                        {
                            _logger.Error(jex, "Utils", "IsValidJson", $"Error occurred in JsonReaderException-IsValidJson(): {jex.Message}");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Utils", "IsValidJson", $"Error occurred in IsValidJson(): {ex.Message}");
                        }
                    }
                }

            });
            return result;
        }
        public async Task<bool> IsValidXml(string strInput, Logger _logger)
        {
            bool result = false;
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(strInput) && strInput.TrimStart().StartsWith('<'))
                {
                    try
                    {
                        XDocument.Parse(strInput);
                        result = true;

                    }
                    catch (Exception ex)
                    {

                        _logger.Error(ex, "Utils", "IsValidJson", $"Error occurred in IsValidXml(): {ex.Message}");
                    }
                }
            });
            return result;
        }
        public async Task InitializeAsync(Logger _logger)
        {
            _logger.Info("Utils", "InitializeAsync", $"Started.");
            try
            {
                await Task.Run(() =>
                {
                    if (_idbConnection == null)
                    {
                        _logger.Info("Utils", "InitializeAsync", "Re-Initialize the DbConnection Connection.");
                        _idbConnection = _connectCustom!.GetSingletonIDbConnection(_logger);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Utils", "InitializeAsync", ex.Message);
                throw;
            }
            _logger.Info("Utils", "InitializeAsync", $"Completed.");
        }
        public async Task WriteTextToFileAsync(string filePath, string text, Logger _logger)
        {
            _logger.Info("Utils", "WriteTextToFileAsync", $"Started.");
            try
            {
                // Use StreamWriter to write text to a file
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(text);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Utils", "WriteTextToFileAsync", ex.Message);
            }
            _logger.Info("Utils", "WriteTextToFileAsync", $"Completed.");
        }
        public async Task<bool> CreateDirectoryAsync(string directoryName, Logger _logger)
        {
            bool result = false;
            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName!);
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "CreateDirectoryAsync", $"Error occurred in CreateDirectoryAsync(): {ex.Message}");
                }
            });
            return result;
        }
        public async Task<bool> DeleteFileAsync(string file, Logger _logger)
        {
            bool result = false;
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "DeleteFileAsync", $"Error occurred in DeleteFileAsync(): {ex.Message}");
                }
            });
            return result;
        }
        public async Task<bool> WriteXmlFileAsync(string xml, string file, Logger _logger)
        {
            bool result = false;
            await Task.Run(() =>
            {
                try
                {
                    Encoding encoding = Encoding.ASCII;
                    byte[] byteArray = encoding.GetBytes(xml);
                    string xmlStringCTD = Encoding.ASCII.GetString(byteArray);
                    Encoding outputEnc = Encoding.ASCII;

#pragma warning disable S3966 // Objects should not be disposed more than once
                    using (StreamWriter sw = new(file, false, outputEnc))  // Opening and Writing data's into the file using stream writer.
                    {
                        sw.WriteLine(xmlStringCTD);
                        sw.Close();
                        sw.Dispose();
                    }
#pragma warning restore S3966 // Objects should not be disposed more than once
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "WriteXmlFileAsync", $"Error occurred in WriteXmlFileAsync(): {ex.Message}");
                }
            });
            return result;
        }
        public async Task<string> ReadXmlFileAsync(string file, Logger _logger)
        {
            string encryptedXml = string.Empty;
            await Task.Run(() =>
            {
                try
                {
#pragma warning disable S3966 // Objects should not be disposed more than once
                    using StreamReader? streamReader = new(file!);
#pragma warning restore S3966 // Objects should not be disposed more than once
                    encryptedXml = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "ReadXmlFileAsync", $"Error occurred in WriteXmlFileAsync(): {ex.Message}");
                }
            });
            return encryptedXml;
        }
        public async Task<string> GetLoadXmlStringAsync(string xml, Logger _logger)
        {
            string result = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xml);
                    result = xmlDocument.OuterXml;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Utils", "GetLoadXmlStringAsync", $"Error occurred in GetLoadXmlFileAsync(): {ex.Message}");
                }
            });
            return result;
        }
        public async Task<bool> CopyFileAsync(string sourceFilePath, string destinationFilePath, Logger _logger, bool overwrite = false)
        {
            bool result = false;
            try
            {
                await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite));
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Utils", "CopyFileAsync", $"Error occurred in CopyFileAsync(): {ex.Message}");
            }
            return result;
        }
        public async Task<bool> MoveFileAsync(string sourceFilePath, string destinationFilePath, Logger _logger, bool overwrite = false)
        {
            bool result = false;
            try
            {
                await Task.Run(() => File.Move(sourceFilePath, destinationFilePath, overwrite));
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Utils", "MoveFileAsync", $"Error occurred in CopyFileAsync(): {ex.Message}");
            }
            return result;
        }
        public async Task<bool> IsFileLocked(string filePath, Logger _logger)
        {
            bool result = false;
            try
            {
                _logger.Info("Utils", "IsFileLocked", $"Debug: Checking if the file is locked: {filePath}");
                FileInfo file = new(filePath);
                using FileStream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                fileStream.Close();
                await fileStream.DisposeAsync();
                _logger.Info("Utils", "IsFileLocked", $"Debug: The file is not locked: {filePath}");
            }
            catch (Exception ex)
            {
                result = true;
                _logger.Error(ex, "Utils", "IsFileLocked", $"Error occurred while checking if the file is locked: {filePath}, Error: {ex.Message}");
            }
            return result;
        }
        public async Task<string> ReadFileAsync(string filePath, Logger _logger)
        {
            string fileTextContent = string.Empty;
            try
            {
                _logger.Info("Utils", "ReadFileAsync", $"Reading file asynchronously: {filePath}");
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader reader = new(fileStream);
                fileTextContent = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Utils", "ReadFileAsync", $"Error occurred while reading file asynchronously: {ex.Message}");
            }
            return fileTextContent;
        }
        public async Task DeleteFiles(Logger _logger, params string[] filePaths)
        {
            try
            {
                foreach (string filePath in filePaths)
                {
                    _logger.Info("Utils", "DeleteFiles", $"Deleting file: {filePath}");
                    await DeleteFileAsync(filePath, _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Utils", "DeleteFiles", $"Error occurred in DeleteFiles(): {ex.Message}");
            }
        }
        public async Task CreateDirectories(Logger _logger, params string[] directories)
        {
            try
            {
                foreach (string directory in directories)
                {
                    _logger.Info("Utils", "CreateDirectories", $"Creating directory: {directory}");
                    await CreateDirectoryAsync(directory, _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Utils", "CreateDirectories", $"Error occurred in CreateDirectories(): {ex.Message}");
            }
        }
        public async Task SaveEmailAsync(string refno, string module, string classname, string method, string code, string desc, Logger _logger)
        {
            try
            {
                _logger.Info("SqlData", "SaveEmailAsync", $"Started.");
                var parameters = new DynamicParameters();
                parameters.Add("@Mail_RefNo", refno, DbType.String);
                parameters.Add("@Mail_Module", module, DbType.String);
                parameters.Add("@Mail_ServiceName", "UAEIPP_Inward_MXMT_Worker", DbType.String);

                parameters.Add("@Mail_ClassName", classname, DbType.String);
                parameters.Add("@Mail_MethodName", method, DbType.String);
                parameters.Add("@Mail_ResponseCode", code, DbType.String);
                parameters.Add("@Mail_Description", desc, DbType.String);
                await _idbConnection.QueryAsync<string>("IPP_Insert_sp_SendMail", parameters, commandTimeout: 1200, commandType: CommandType.StoredProcedure, transaction: null);
                _logger.Info("SqlData", "SaveEmailAsync", $"SaveEmailAsync Data is Inserted.");

            }
            catch (Exception ex)
            {
                _logger.Info("SqlData", "SaveEmailAsync", ex.Message);
                throw new InvalidOperationException(ex.Message);
            }

        }
        public async Task<string> GetReplacePacsXml(string xml, Logger _logger)
        {
            string result = string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    _logger.Info("Utils", "GetReplacePacsXml", $"GetReplacedPacsXml is invoked.");



                    xml = xml.Replace("<AppHdr xmlns=\"urn:iso:std:iso:20022:tech:xsd:head.001.001.02\">",
                     "<AppHdr xmlns=\"urn:iso:std:iso:20022:tech:xsd:head.001.001.02\" " +
                     "xmlns:Saa=\"urn:swift:saa:xsd:saa.2.0\" " +
                     "xmlns:Sw=\"urn:swift:snl:ns.Sw\" " +
                     "xmlns:SwGbl=\"urn:swift:snl:ns.SwGbl\" " +
                     "xmlns:SwInt=\"urn:swift:snl:ns.SwInt\" " +
                     "xmlns:SwSec=\"urn:swift:snl:ns.SwSec\">");

                    xml = xml.Replace("<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08\">",
                                      "<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08\" " +
                                      "xmlns:Saa=\"urn:swift:saa:xsd:saa.2.0\" " +
                                      "xmlns:Sw=\"urn:swift:snl:ns.Sw\" " +
                                      "xmlns:SwGbl=\"urn:swift:snl:ns.SwGbl\" " +
                                      "xmlns:SwInt=\"urn:swift:snl:ns.SwInt\" " +
                                      "xmlns:SwSec=\"urn:swift:snl:ns.SwSec\">");

                    //xml = xml.Replace("<AppHdr xmlns=\"urn:iso:std:iso:20022:tech:xsd:head.001.001.02\">",
                    //"<AppHdr xmlns=\"urn:iso:std:iso:20022:tech:xsd:head.001.001.02\">");

                    //xml = xml.Replace("<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08\">",
                    //                  "<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08\">");

                    result = xml;
                    _logger.Info("Utils", "GetReplacePacsXml", $"GetReplacedPacsXml is done.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Utils", "GetReplacePacsXml", $"Error occurred in GetReplacedPacsXml(): {ex.Message}");
                }
            });
            return result;
        }
        public static bool IBANValidate(string input, Logger _logger)
        {
            bool isValid = false;

            try
            {
                string pattern = @"^[A-Z]{2}[0-9]{2}[a-zA-Z0-9]{1,30}$";
                isValid = Regex.IsMatch(input, pattern);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occurred in IBANValidate(): ErrorMessage: {ex.Message}, StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
            }
            return isValid;
        }
        public static T RemoveNullOrEmptyProperties<T>(T obj, Logger logger)
        {
            try
            {
                if (obj == null) return obj;

                Type type = obj.GetType();

                // Return primitive types as they are
                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                {
                    return obj;
                }

                if (obj is IEnumerable enumerable)
                {
                    var listType = obj.GetType();
                    if (listType.IsGenericType)
                    {
                        Type genericType = listType.GetGenericArguments()[0];
                        var cleanList = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType)) as IList;

                        foreach (var item in enumerable)
                        {
                            try
                            {
                                var cleanedItem = RemoveNullOrEmptyProperties(item, logger);
                                if (cleanedItem != null)
                                {
                                    cleanList?.Add(cleanedItem);
                                }
                            }
                            catch
                            {
                                cleanList?.Add(item); // Add original item if cleaning fails
                            }
                        }

                        return cleanList != null && cleanList.Count > 0 ? (T)(object)cleanList : default;
                    }
                }

                bool isObjectEmpty = true;

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.CanRead || !property.CanWrite) continue;

                    try
                    {
                        var value = property.GetValue(obj);

                        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                        {
                            property.SetValue(obj, null);
                        }
                        else
                        {
                            var cleanedValue = RemoveNullOrEmptyProperties(value, logger);
                            property.SetValue(obj, cleanedValue);

                            if (cleanedValue != null)
                            {
                                isObjectEmpty = false;
                            }
                        }
                    }
                    catch
                    {
                        // If property access fails, skip it but continue processing others
                        continue;
                    }
                }

                return isObjectEmpty ? default : obj;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred in RemoveNullOrEmptyProperties(): ErrorMessage: {ex.Message}, StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                return obj;
            }
        }
    }
}
