using Microsoft.Extensions.Options;
using NLogFluent;
using Pacs008.Request.Model;
using Raqmiyat.Framework.Custom;
using Raqmiyat.Framework.Domain;
using Raqmiyat.Framework.Model;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using UAEIPP_Inward_MXMT_Worker.Model;

namespace UAEIPP_Inward_MXMT_Worker
{
    public class MXtoMTConversionWorker : BackgroundService
    {
        private readonly NLogMXtoMTConversionWorker _logger;
        private IOptions<ServiceParams> _serviceParams;
        private readonly Utils _utils;
        private readonly Conversion _conversion;
        private readonly SqlData _sqlData;
        private readonly IOptions<FolderPath> _folderPath;
        public MXtoMTConversionWorker(NLogMXtoMTConversionWorker logger, IOptions<ServiceParams> serviceParams, Utils utils, Conversion conversion, SqlData sqlData, IOptions<FolderPath> folderPath)
        {
            _logger = logger;
            _serviceParams = serviceParams;
            _utils = utils;
            _conversion = conversion;
            _sqlData = sqlData;
            _folderPath = folderPath;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.Info("MXtoMTConversionWorker", "ExecuteAsync", $"MXtoMTConversionWorker starting at: {DateTimeOffset.Now}");
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _utils.InitializeAsync(_logger._log);
                    await ProcessAsync();
                    await Task.Delay(TimeSpan.FromMilliseconds(_serviceParams.Value.TimeoutMilliSeconds), stoppingToken);
                }
                _logger.Info("MXtoMTConversionWorker", "ExecuteAsync", $"MXtoMTConversionWorker stopping at: {DateTimeOffset.Now}");
            }
            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "ExecuteAsync", $"Error occurred in ExecuteAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                await _utils.SaveEmailAsync("MXtoMTConversionWorker", "Inward", "MXtoMTConversionWorker", "ExecuteAsync", "1001", ex.Message, _logger._log);
            }
        }
        private async Task ProcessAsync()
        {
            _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"ProcessAsync is invoked.");
            try
            {
                var dbParamsRoot = await _sqlData.GetDataFromDatabaseMttoMx(_logger._log);
                if (dbParamsRoot != null && dbParamsRoot.DBRequestHeader != null && dbParamsRoot.DBRequestDetails != null)
                {
                    _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"RefenceNbr: {dbParamsRoot.DBRequestDetails!.FirstOrDefault()!.RefenceNbr!}, Count: {dbParamsRoot.DBRequestDetails!.Count}");
                    string paymentHeaderStatus = "FG";
                    if (dbParamsRoot != null && dbParamsRoot.DBRequestHeader != null && dbParamsRoot.DBRequestDetails != null)
                    {
                        foreach (var dbParams in dbParamsRoot.DBRequestDetails)
                        {
                            if (dbParams.EndToEnd_Identification == null)
                            {
                                paymentHeaderStatus = "IBR";
                                continue;
                            }
                            _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"Payment started. EndToEndIdentification: {dbParams.EndToEnd_Identification},SrlNbr:{dbParams.SrlNbr}");
                            string paymentDetailsStatus = "20";
                            try
                            {
                                //if (dbParams.Payment_Type!.ToUpper() == "SWIFTPAYMENT")
                                //{
                                //    var swiftMessage = await _conversion.TransformMXToMTAsync(dbParams);
                                //    _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"Payment done. SrlNbr: {dbParams.SrlNbr}, PaymentDetailsStatus: {paymentDetailsStatus}");
                                //    await _sqlData.UpdateBatchPaymentDetailsAsync(dbParams.RefenceNbr!, dbParams.SrlNbr, paymentDetailsStatus, paymentHeaderStatus, swiftMessage, _logger._log);
                                //}
                                bool isMtorMxCoreBank = _serviceParams.Value.MtorMxCoreBank;
                                if (dbParams.Payment_Type!.ToUpper() == "SWIFTPAYMENT" && isMtorMxCoreBank)
                                {
                                    var swiftMessage = await _conversion.TransformMXToMTAsync(dbParamsRoot);
                                    _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"Payment done. EndToEndIdentification:{dbParams.EndToEnd_Identification}, SrlNbr: {dbParams.SrlNbr}, PaymentDetailsStatus: {paymentDetailsStatus}");
                                    await _sqlData.UpdateBatchPaymentDetailsAsync(dbParams.RefenceNbr!, dbParams.SrlNbr, paymentDetailsStatus, paymentHeaderStatus, "", swiftMessage, dbParams.Creditor_IBAN!, dbParams.EndToEnd_Identification, _logger._log);
                                }
                                else
                                {
                                    var xml = await TransformToMXAsync(dbParamsRoot);
                                    _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"XML:{xml}");
                                    try
                                    {
                                        if (xml != null)
                                        {
                                            List<string> validationError = new List<string>();
                                            if (_serviceParams.Value.IsXMLValidation)
                                            {
                                                validationError = ValidateXml(xml);
                                            }

                                            if (validationError.Count <= 0)
                                            {
                                                string finalXml = string.Empty;
                                                Body body = await _utils.Deserialize<Body>(xml, _logger._log);
                                                if (_serviceParams.Value.IsDataPDU)
                                                {
                                                    var DataPDUObject = await ConvertCtdToPacs008DataPDU(body);
                                                    finalXml = await _utils.SerializeToDataPDUXML(DataPDUObject, _logger._log);
                                                }
                                                else
                                                {
                                                    finalXml = await _utils.SerializeToXML(body, _logger._log);
                                                }
                                                _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"XML validation success: {finalXml}");
                                                string XsdPaymentStatus = "SUC";
                                                await _sqlData.UpdateBatchPaymentDetailsAsync(
                                                    dbParams.RefenceNbr!, dbParams.SrlNbr, paymentDetailsStatus, paymentHeaderStatus, XsdPaymentStatus, finalXml,
                                                    dbParams.Creditor_IBAN!, dbParams.EndToEnd_Identification, _logger._log);
                                                _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"XML validation successful. Proceeding with database update XsdStatus:{XsdPaymentStatus}");
                                            }
                                            else
                                            {
                                                Body body = await _utils.Deserialize<Body>(xml, _logger._log);
                                                var finalXml = await _utils.SerializeToXML(body, _logger._log);
                                                _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"XML validation failed: {finalXml}");
                                                string headerStatus = "ERR";
                                                string PaymentStatus = "25";
                                                await _sqlData.UpdateValidationStatusAsync(dbParams.RefenceNbr!, dbParams.SrlNbr, PaymentStatus, headerStatus, dbParams.Creditor_IBAN!, String.Join(',', validationError), _logger._log);
                                                _logger.Error("MXtoMTConversionWorker", "ProcessAsync", $"XML validation failed. Error: {String.Join(',', validationError)},XsdStatus:{headerStatus}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error("MXtoMTConversionWorker", "ProcessAsync", $"XML validation failed: {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                                        string headerStatus = "ERR";
                                        string PaymentStatus = "25";
                                        await _sqlData.UpdateValidationStatusAsync(dbParams.RefenceNbr!, dbParams.SrlNbr, PaymentStatus, headerStatus, dbParams.Creditor_IBAN!, "", _logger._log);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                paymentDetailsStatus = "10";
                                paymentHeaderStatus = "IBR";
                                _logger.Error("MXtoMTConversionWorker", "ProcessAsync", $"Error occurred in Foreach-ProcessAsync(): {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                                await _sqlData.UpdateBatchPaymentDetailsAsync(dbParams.RefenceNbr!, dbParams.SrlNbr, paymentDetailsStatus, "", "", paymentHeaderStatus, "", dbParams.EndToEnd_Identification, _logger._log);
                            }
                        }
                    }
                }
                _logger.Info("MXtoMTConversionWorker", "ProcessAsync", $"ProcessAsync is done.");
            }

            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "ProcessAsync", $"Error occurred in ProcessAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                await _utils.SaveEmailAsync("MXtoMTConversionWorker", "Inward", "MXtoMTConversionWorker", "ProcessAsync", "1002", ex.Message, _logger._log);
            }
        }

        private async Task<string> TransformToMXAsync(DBParamsRoot dbParamsRoot)
        {
            string xml = string.Empty;
            try
            {
                _logger.Info("MXtoMTConversionWorker", "TransformToMXAsync", $"TransformToMXAsync is invoked.");
                if (dbParamsRoot != null && dbParamsRoot.DBRequestHeader != null && dbParamsRoot.DBRequestDetails != null)
                {

                    var body = new Body();
                    var apphdrtag = await GetApphdr(dbParamsRoot);
                    var document = new Document();
                    var fIToFICstmrCdtTrf = new FIToFICstmrCdtTrf();
                    var grpHdrTask = await GetGrpHdrAsync(dbParamsRoot.DBRequestHeader!, dbParamsRoot.DBRequestDetails);
                    var cdtTrfTxInfTask = GetTransactionInfosAsync(dbParamsRoot.DBRequestDetails!, dbParamsRoot.DBRequestHeader!);
                    fIToFICstmrCdtTrf.GrpHdr = grpHdrTask;
                    fIToFICstmrCdtTrf.CdtTrfTxInf = cdtTrfTxInfTask;
                    document.FIToFICstmrCdtTrf = fIToFICstmrCdtTrf;
                    body.AppHdr = apphdrtag;
                    body.Document = document;
                    var finalxmlobject = Utils.RemoveNullOrEmptyProperties(body, _logger._log);
                    xml = await _utils.SerializeToXML(finalxmlobject, _logger._log);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        xml = await _utils.GetReplacePacsXml(xml, _logger._log);
                       _logger.Info("MXtoMTConversionWorker", "TransformToMXAsync", $"TransformToMXAsync xml:{xml}");
                    }
                }
                _logger.Info("MXtoMTConversionWorker", "TransformToMXAsync", $"TransformToMXAsync is done.");
            }
            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "TransformToMXAsync", $"Error occurred in TransformToMXAsync(): {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                await _utils.SaveEmailAsync(dbParamsRoot.DBRequestHeader!.Message_Identification!, "Inward", "MXtoMTConversionWorker", "TransformToMXAsync", "1003", ex.Message, _logger._log);
            }
            return xml;
        }
        private List<string> ValidateXml(string xml)
        {
            List<string> errors = new List<string>();

            var xsdFiles = Directory.GetFiles(_folderPath.Value.xsdPath!, _folderPath.Value.xsdFileName!);

            XmlSchemaSet schemaSet = new XmlSchemaSet();

            foreach (var xsdFile in xsdFiles)
            {
                schemaSet.Add("urn:iso:std:iso:20022:tech:xsd:pacs.008.001.08", xsdFile);
            }

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema
            };

            settings.ValidationEventHandler += (sender, args) =>
            {
                errors.Add(args.Message);
                _logger.Error("MXtoMTConversionWorker", "ValidateXml", $"XML Validation Error: {args.Message}");
            };

            try
            {
                xml = xml.Trim();

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                using (XmlReader reader = XmlReader.Create(ms, settings))
                {
                    while (reader.Read()) { }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "ValidateXml", $"Validation failed: {ex.Message}");
            }
            return errors;
        }

        private async Task<AppHdr> GetApphdr(DBParamsRoot DBParamsRoot)
        {
            var AppHdr = new AppHdr();
            await Task.Run(async () =>
            {
                try
                {
                    _logger.Info("MXtoMTConversionWorker", "GetApphdr", $"GetApphdr is invoked.");
                    if (DBParamsRoot != null)
                    {
                        Fr fr = new()
                        {
                            FIId = new FIId
                            {
                                FinInstnId = new FinInstnId
                                {
                                    BICFI = DBParamsRoot!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification
                                }
                            }
                        };
                        AppHdr.Fr = fr;
                        To to = new()
                        {
                            FIId = new FIId
                            {
                                FinInstnId = new FinInstnId
                                {
                                    BICFI = DBParamsRoot!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification
                                }
                            }
                        };
                        AppHdr.To = to;
                        AppHdr.BizMsgIdr = DBParamsRoot!.DBRequestDetails!.FirstOrDefault()!.EndToEnd_Identification;
                        AppHdr.MsgDefIdr = _serviceParams.Value.PacsMsg;
                        AppHdr.BizSvc = "swift.cbprplus.02";
                        //AppHdr.CreDt = Convert.ToDateTime(DBParamsRoot.DBRequestHeader!.Creation_DateTime);
                        AppHdr.CreDt = DBParamsRoot.DBRequestHeader!.Creation_DateTime!;

                    }
                    _logger.Info("MXtoMTConversionWorker", "GetApphdr", $"GetApphdr is done.");
                }
                catch (Exception ex)
                {
                    _logger.Error("MXtoMTConversionWorker", "GetApphdr", $"Error occurred in GetApphdr():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                    await _utils.SaveEmailAsync(AppHdr.MsgDefIdr!, "Inward", "MXtoMTConversionWorker", "GetApphdr", "1004", ex.Message, _logger._log);
                }

            });
            return AppHdr;
        }
        private async Task<GrpHdr> GetGrpHdrAsync(DBRequestHeader dBRequestHeader, List<DBRequestDetails> dBRequestDetails)
        {
            var grpHdr = new GrpHdr();
            await Task.Run(async () =>
            {
                try
                {
                    _logger.Info("MXtoMTConversionWorker", "GetGrpHdrAsync", $"GetGrpHdrAsync is invoked.");
                    if (dBRequestHeader != null)
                    {
                        string SttlmAcctIban = string.Empty;
                        string SttlmAcctOtrId = string.Empty;

                        if (!string.IsNullOrEmpty(_serviceParams.Value.SttlmAcct))
                        {
                            if (Utils.IBANValidate(_serviceParams.Value.SttlmAcct!.Replace("/", ""), _logger._log))
                            {
                                SttlmAcctIban = _serviceParams.Value.SttlmAcct;
                            }
                            else
                            {
                                SttlmAcctOtrId = _serviceParams.Value.SttlmAcct;
                            }
                        }

                        SttlmInf sttlmInf = new()
                        {

                            SttlmMtd = _serviceParams.Value.SttlmMtd,
                            SttlmAcct = new SttlmAcct()
                            {
                                Id = new Id()
                                {
                                    IBAN = SttlmAcctIban,
                                    Othr = new Othr()
                                    {
                                        Id = SttlmAcctOtrId
                                    }
                                },
                            },
                            InstgRmbrsmntAgt = new InstgRmbrsmntAgt
                            {
                                FinInstnId = new FinInstnId
                                {
                                    BICFI = dBRequestHeader!.Instructing_Agent_FI_ID
                                }
                            },
                            InstdRmbrsmntAgt = new InstdRmbrsmntAgt
                            {
                                FinInstnId = new FinInstnId
                                {
                                    BICFI = dBRequestHeader!.Instructing_Agent_FI_ID
                                }
                            },
                        };

                        grpHdr.MsgId = dBRequestDetails.FirstOrDefault()!.EndToEnd_Identification;
                        grpHdr.CreDtTm = dBRequestHeader!.Creation_DateTime;
                        grpHdr.NbOfTxs = "1";
                        grpHdr.SttlmInf = sttlmInf;
                    }
                    _logger.Info("MXtoMTConversionWorker", "GetGrpHdrAsync", $"GetGrpHdrAsync is done.");
                }
                catch (Exception ex)
                {
                    _logger.Error("MXtoMTConversionWorker", "GetGrpHdrAsync", $"Error occurred in GetGrpHdrAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                    await _utils.SaveEmailAsync(grpHdr.MsgId!, "Inward", "MXtoMTConversionWorker", "GetGrpHdrAsync", "1005", ex.Message, _logger._log);
                }

            });
            return grpHdr;
        }
        private List<CdtTrfTxInf> GetTransactionInfosAsync(List<DBRequestDetails> DBRequestDetails,DBRequestHeader dBRequestHeader)
        {
            var cdtTrfTxInfs = new List<CdtTrfTxInf>();
            string msg = string.Empty;
            string interbanksettlementdate = dBRequestHeader.Interbank_Settlement_Date!;
            try
            {
                _logger.Info("MXtoMTConversionWorker", "GetTransactionInfosAsync", $"GetTransactionInfosAsync is invoked.");
                foreach (DBRequestDetails dbRequestDetails in DBRequestDetails!)
                {
                    try
                    {
                        msg = dbRequestDetails.UETR!;
                        cdtTrfTxInfs.Add(GetSingleTxInfsAsync(dbRequestDetails, _serviceParams.Value.RmtInf!,interbanksettlementdate));
                    }
                    catch (Exception ex)
                    {
                       _logger.Error("MXtoMTConversionWorker", "GetTransactionInfosAsync", $"Error occurred in GetTransactionInfosAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                    }
                  
                }
                _logger.Info("MXtoMTConversionWorker", "GetTransactionInfosAsync", $"GetTransactionInfosAsync is done.");
            }
            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "GetGrpHdrAsync", $"Error occurred in GetGrpHdrAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
            }

            return cdtTrfTxInfs;
        }
        private  CdtTrfTxInf GetSingleTxInfsAsync(DBRequestDetails dbRequestDetails, string RmtInf,string interbanksettlementdate)
        {
            CdtTrfTxInf cdtTrfTxInf = new CdtTrfTxInf();
            try
            {
                _logger.Info("MXtoMTConversionWorker", "GetSingleTxInfsAsync", $"GetSingleTxInfsAsync is invoked.");
                cdtTrfTxInf.PmtId = GetPmtId(dbRequestDetails);
                //cdtTrfTxInf.PmtTpInf = GetPmtTpInf(dbRequestDetails);
                cdtTrfTxInf.IntrBkSttlmAmt = GetIntrBkSttlmAmt(dbRequestDetails);
                cdtTrfTxInf.IntrBkSttlmDt = interbanksettlementdate;
                cdtTrfTxInf.ChrgBr = "CRED";//dbRequestDetails.Charge_Bearer;
                cdtTrfTxInf.InstgAgt = GetInstgAgt(dbRequestDetails);
                cdtTrfTxInf.InstdAgt = GetInstdAgt(dbRequestDetails);
                cdtTrfTxInf.Dbtr = GetDbtr(dbRequestDetails);
                cdtTrfTxInf.DbtrAcct = GetDbtrAcct(dbRequestDetails);
                cdtTrfTxInf.DbtrAgt = GetDbtrAgt(dbRequestDetails);
                cdtTrfTxInf.CdtrAgt = GetCdtrAgt(dbRequestDetails);
                cdtTrfTxInf.Cdtr = GetCdtr(dbRequestDetails);
                cdtTrfTxInf.CdtrAcct = GetCdtrAcct(dbRequestDetails);
                cdtTrfTxInf.Purp = GetPurp(dbRequestDetails);
                cdtTrfTxInf.RmtInf = GetRmtInf(dbRequestDetails, RmtInf);
               _logger.Info("MXtoMTConversionWorker", "GetSingleTxInfsAsync", $"GetSingleTxInfsAsync is done.");
            }
            
            catch (Exception ex)
            {
                _logger.Error("MXtoMTConversionWorker", "GetGrpHdrAsync", $"Error occurred in GetGrpHdrAsync():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
            }
            return cdtTrfTxInf;
        }

        private static Dbtr GetDbtr(DBRequestDetails dbRequestDetails)
        {
            Dbtr dbtr = new Dbtr();
            dbtr.Nm = dbRequestDetails.Debtor_Name;
            dbtr.Id = GetDebtorOrg(dbRequestDetails);
            //dbtr.CtctDtls = GetCtctDtls();
            return dbtr;
        }

        private static Purp GetPurp(DBRequestDetails dbRequestDetails)
        {
            Purp purp = new Purp();
            purp.Prtry = dbRequestDetails.Category_Purpose_Code;
            return purp;
        }
        private static RmtInf GetRmtInf(DBRequestDetails dbRequestDetails, string RmtInf)
        {
            RmtInf rmtInf = new RmtInf();
            if (!string.IsNullOrEmpty(RmtInf))
            {
                rmtInf.Ustrd = string.Concat(RmtInf, dbRequestDetails.Remittance_Information ?? "");
            }
            else
            {
                rmtInf.Ustrd = dbRequestDetails.Remittance_Information ?? "";
            }
            return rmtInf;
        }
        private static CdtrAcct GetCdtrAcct(DBRequestDetails dbRequestDetails)
        {
            CdtrAcct cdtrAcct = new CdtrAcct();

            Id id2 = new Id();
            if (dbRequestDetails.Creditor_Acc_Not_IBAN!.ToUpper() == "Y")
            {
                Othr debtorOthr = new Othr();
                debtorOthr.Id = dbRequestDetails.Creditor_IBAN;
                SchmeNm debtorSchmeName = new SchmeNm();
                debtorSchmeName.Cd = "AIIN";
                debtorOthr.SchmeNm = debtorSchmeName;
                id2.Othr = debtorOthr;
                cdtrAcct.Id = id2;
            }
            else
            {
                id2.IBAN = dbRequestDetails.Creditor_IBAN;
                cdtrAcct.Id = id2;
            }
            return cdtrAcct;
        }

        private static Cdtr GetCdtr(DBRequestDetails dbRequestDetails)
        {
            Cdtr cdtr = new Cdtr();
            cdtr.Nm = dbRequestDetails.Creditor_Name;
            return cdtr;
        }

        private static CtctDtls GetCtctDtls()
        {
            CtctDtls ctctDtls = new CtctDtls();
            ctctDtls.NmPrfx = Convert.ToString(NamePrefix2Code.MIST);
            return ctctDtls;
        }

        private static DbtrAcct GetDbtrAcct(DBRequestDetails dbRequestDetails)
        {
            DbtrAcct dbtrAcct = new DbtrAcct();

            Id id1 = new Id();
            if (dbRequestDetails.Debtor_Acc_Not_IBAN!.ToUpper() == "Y")
            {
                Othr debtorOthr = new Othr();
                debtorOthr.Id = dbRequestDetails.Debtor_IBAN;
                SchmeNm debtorSchmeName = new SchmeNm();
                debtorSchmeName.Cd = "AIIN";
                debtorOthr.SchmeNm = debtorSchmeName;
                id1.Othr = debtorOthr;
                dbtrAcct.Id = id1;
            }
            else
            {
                id1.IBAN = dbRequestDetails.Debtor_IBAN;
                dbtrAcct.Id = id1;
            }
            Tp tp1 = new Tp();
            tp1.Cd = dbRequestDetails.Debtor_Account_Type;
            dbtrAcct.Tp = tp1;

            return dbtrAcct;
        }

        private static CdtrAgt GetCdtrAgt(DBRequestDetails dbRequestDetails)
        {
            CdtrAgt cdtrAgt = new CdtrAgt();
            FinInstnId finInstnId2 = new FinInstnId();
            finInstnId2.BICFI = dbRequestDetails.Creditor_Institution_Identification;
            cdtrAgt.FinInstnId = finInstnId2;
            return cdtrAgt;
        }

        private static DbtrAgt GetDbtrAgt(DBRequestDetails dbRequestDetails)
        {
            DbtrAgt dbtrAgt = new DbtrAgt();
            FinInstnId finInstnId1 = new FinInstnId();
            finInstnId1.BICFI = dbRequestDetails.Debtor_Institution_Identification;
            dbtrAgt.FinInstnId = finInstnId1;
            return dbtrAgt;
        }
        private static InstdAgt GetInstdAgt(DBRequestDetails dbRequestDetails)
        {
            InstdAgt InstdAgt = new InstdAgt();
            FinInstnId finInstnId1 = new FinInstnId();
            finInstnId1.BICFI = dbRequestDetails.Creditor_Institution_Identification;
            InstdAgt.FinInstnId = finInstnId1;
            return InstdAgt;
        }
        private static InstgAgt GetInstgAgt(DBRequestDetails dbRequestDetails)
        {
            InstgAgt InstgAgt = new InstgAgt();
            FinInstnId finInstnId1 = new FinInstnId();
            finInstnId1.BICFI = dbRequestDetails.Debtor_Institution_Identification;
            InstgAgt.FinInstnId = finInstnId1;
            return InstgAgt;
        }

        private static Id GetDebtorPvt(DBRequestDetails dbRequestDetails)
        {
            Id id = new Id();
            PrvtId pvrtId = new PrvtId();
            DtAndPlcOfBirth dtAndPlcOfBirth = new DtAndPlcOfBirth();
            dtAndPlcOfBirth.BirthDt = dbRequestDetails.Debtor_BirthDate;
            dtAndPlcOfBirth.CityOfBirth = dbRequestDetails.Debtor_CityOfBirth;
            dtAndPlcOfBirth.CtryOfBirth = dbRequestDetails.Debtor_CountryOfBirth;
            pvrtId.DtAndPlcOfBirth = dtAndPlcOfBirth;
            Othr othr = new Othr();
            othr.Id = dbRequestDetails.Debtor_Identification;
            SchmeNm schmeNm = new SchmeNm();
            schmeNm.Cd = dbRequestDetails.Debtor_Identity_Type;
            othr.SchmeNm = schmeNm;
            othr.Issr = dbRequestDetails.Debtor_Issuer;
            pvrtId.Othr = othr;
            id.PrvtId = pvrtId;
            return id;
        }

        private static Id GetDebtorOrg(DBRequestDetails dbRequestDetails)
        {
            Id id = new Id();

            if (dbRequestDetails.Debtor_Identification_Code == "BOID")
            {
                OrgId orgId = new OrgId();
                Othr othr = new Othr();
                othr.Id = dbRequestDetails.Debtor_Identification;
                SchmeNm schmeNm = new SchmeNm();
                schmeNm.Cd = dbRequestDetails.Debtor_Identification_Code;
                othr.SchmeNm = schmeNm;
                othr.Issr = dbRequestDetails.Debtor_Issuer;
                orgId.Othr = othr;
                id.OrgId = orgId;
            }
            else
            {
                id = GetDebtorPvt(dbRequestDetails);
            }
            return id;
        }

        private static IntrBkSttlmAmt GetIntrBkSttlmAmt(DBRequestDetails dbRequestDetails)
        {
            IntrBkSttlmAmt intrBkSttlmAmt = new IntrBkSttlmAmt();
            intrBkSttlmAmt.Ccy = dbRequestDetails.Active_Currency;
            intrBkSttlmAmt.Text = dbRequestDetails.Interbank_Settlement_Amount;
            return intrBkSttlmAmt;
        }

        //private static PmtTpInf GetPmtTpInf(DBRequestDetails dbRequestDetails)
        //{
        //    PmtTpInf pmtTpInf = new PmtTpInf();
        //    CtgyPurp ctgyPurp = new CtgyPurp();
        //    ctgyPurp.Prtry = dbRequestDetails.Category_Purpose_Code;
        //    pmtTpInf.CtgyPurp = ctgyPurp;
        //    return pmtTpInf;
        //}

        private static PmtId GetPmtId(DBRequestDetails dbRequestDetails)
        {
            PmtId pmtId = new PmtId();
            pmtId.InstrId = dbRequestDetails.Instruction_Identification;
            pmtId.EndToEndId = dbRequestDetails.EndToEnd_Identification;
            pmtId.TxId = dbRequestDetails.Transaction_Identification;
            pmtId.UETR = dbRequestDetails.UETR;
            return pmtId;
        }

        private async Task<DataPDUPacs008> ConvertCtdToPacs008DataPDU(Body pacs008Model)
        {
            _logger.Info("MXtoMTConversionWorker", "ConvertCtdToPacs008DataPDU", $"ConvertCtdToPacs008DataPDU is invoked.");

            DataPDUPacs008 dataPDUPacs008 = new DataPDUPacs008();
            await Task.Run(() =>
            {
                try
                {
                    var Header = new Header()
                    {
                        Message = new Message()
                        {
                            SenderReference = pacs008Model.AppHdr!.BizMsgIdr,
                            MessageIdentifier = _serviceParams.Value.PacsMsg,
                            Format = _serviceParams.Value.Format,
                            SubFormat = _serviceParams.Value.SubFormat,
                            Sender = new Party()
                            {
                                DN = $"ou=xxx,o={pacs008Model.AppHdr!.Fr!.FIId!.FinInstnId!.BICFI!.Substring(0, 8).ToLower()},o=swift",
                                FullName = new FullName()
                                {
                                    X1 = pacs008Model.AppHdr!.Fr!.FIId!.FinInstnId!.BICFI,
                                }
                            },
                            Receiver = new Party()
                            {
                                DN = $"ou=xxx,o={pacs008Model.AppHdr!.To!.FIId!.FinInstnId!.BICFI!.Substring(0, 8).ToLower()},o=swift",
                                FullName = new FullName()
                                {
                                    X1 = pacs008Model.AppHdr!.To!.FIId!.FinInstnId!.BICFI,
                                }
                            },
                            InterfaceInfo = new InterfaceInfo()
                            {
                                UserReference = pacs008Model.AppHdr!.BizMsgIdr,
                                MessageCreator = _serviceParams.Value.MessageCreator
                            },
                            NetworkInfo = new NetworkInfo()
                            {
                                Priority = _serviceParams.Value.Priority,
                                Service = _serviceParams.Value.Service,
                                Network = _serviceParams.Value.Network,
                            }
                        }
                    };
                    dataPDUPacs008.Header = Header;
                    dataPDUPacs008.Body = pacs008Model;

                }
                catch (Exception ex)
                {
                    _logger.Error("MXtoMTConversionWorker", "ConvertCtdToPacs008DataPDU", $"Error occurred in ConvertCtdToPacs008DataPDU():  {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                }
            });
            _logger.Info("MXtoMTConversionWorker", "ConvertCtdToPacs008DataPDU", $"ConvertCtdToPacs008DataPDU is done.");
            return dataPDUPacs008;
        }

    }
}