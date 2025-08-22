using Microsoft.Extensions.Options;
using NLogFluent;
using Raqmiyat.Framework.Model;
using System.Text;

namespace Raqmiyat.Framework.Domain
{
    public class Conversion
    {
        private readonly NLogMXtoMTConversionWorker _logger;
        private readonly IOptions<ServiceParams> _serviceParams;

        public Conversion(NLogMXtoMTConversionWorker logger, IOptions<ServiceParams> serviceParams)
        {
            _logger = logger;
            _serviceParams = serviceParams;
        }
        public async Task<string> TransformMXToMTAsync(DBParamsRoot dBBatchPaymentParams)
        {
            StringBuilder sb = new StringBuilder();
            await Task.Run(() =>
            {
                try
                {
                    string interbankSettlementdate = dBBatchPaymentParams!.DBRequestHeader!.Interbank_Settlement_Date!.Replace("-", "");
                  
                    //string interbankSettlementdate = dBBatchPaymentParams!.DBRequestHeader.Interbank_Settlement_Date?
                    //    .ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? string.Empty;

                    _logger.Info("Conversion", "TransformMXToMTAsync", $"Started.");
                    // Construct MT103 header
                    sb.Append("{1:F01").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification).Append("XXXX0000000000}{2:I103").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification).Append("AXXX").Append("0803").Append("XXXX0000000000").Append("N}").Append("{3:{108:000000").Append(interbankSettlementdate).Append("}{121:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.UETR).Append("}}").Append("{4:\n");
                    // Construct sender details
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.EndToEnd_Identification))
                    {
                        sb.Append(":20:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.EndToEnd_Identification).Append("\n");
                    }
                    sb.Append(":23B:CRED\n");
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Category_Purpose_Code))
                    {
                        sb.Append(":26T:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Category_Purpose_Code).Append("\n");
                    }
                    // Construct receiver details
                    if (!string.IsNullOrEmpty(interbankSettlementdate) || !string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Active_Currency) || !string.IsNullOrEmpty( Convert.ToString(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Interbank_Settlement_Amount)))
                    {
                        sb.Append(":32A:").Append(interbankSettlementdate).Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Active_Currency).Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Interbank_Settlement_Amount).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Active_Currency) || !string.IsNullOrEmpty(Convert.ToString(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Interbank_Settlement_Amount)))
                    {
                        sb.Append(":33B:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Active_Currency).Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Interbank_Settlement_Amount).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_IBAN) || !string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Name))
                    {
                        sb.Append(":50K:/").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_IBAN).Append("\n").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Name).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification))
                    {
                        sb.Append(":52D:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification))
                    {
                        sb.Append(":53A:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Debtor_Institution_Identification).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification))
                    {
                        sb.Append(":54A:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification))
                    {
                        sb.Append(":57A:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Institution_Identification).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_IBAN) || !string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Name))
                    {
                        sb.Append(":59:/").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_IBAN).Append("\n").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Creditor_Name).Append("\n");
                    }
                    // Payment details
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Remittance_Information))
                    {
                        sb.Append(":70:").Append(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Remittance_Information).Append("\n");
                    }
                    if (!string.IsNullOrEmpty(dBBatchPaymentParams!.DBRequestDetails!.FirstOrDefault()!.Charge_Bearer))
                    {
                        sb.Append(":71A:").Append(dBBatchPaymentParams.DBRequestDetails!.FirstOrDefault()!.Charge_Bearer).Append("\n");
                    }
                    sb.Append("-}");

                }
                catch (Exception ex)
                {
                    _logger.Error("Conversion", "TransformMXToMTAsync", $"Exception: {ex.Message},StackTrace: {ex.StackTrace}, InnerException: {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                }
                _logger.Info("Conversion", "TransformMXToMTAsync", $"Completed.");
            });
            return sb.ToString();
        }
    }
}
