using Dapper;
using Microsoft.Extensions.Options;
using NLog;
using Raqmiyat.Framework.Model;
using System.Data;

namespace Raqmiyat.Framework.Domain
{
    public class SqlData
    {
        private readonly IOptions<ServiceParams> _serviceParams;
        private readonly IOptions<StoredProcedureParams> _storedProcedureParams;
        private readonly IDbConnection _idbConnection;

        public SqlData(IOptions<ServiceParams> serviceParams, IOptions<StoredProcedureParams> storedProcedureParams, IDbConnection idbConnection)
        {
            _serviceParams = serviceParams;
            _storedProcedureParams = storedProcedureParams;
            _idbConnection = idbConnection;
        }
        public async Task<List<DBBatchPaymentParams>> GetDataFromDatabaseAsync(Logger _logger)
        {
            _logger.Info("SqlData", "GetDataFromDatabaseAsync", $"Started.");
            var dbBatchPaymentParams = new List<DBBatchPaymentParams>();
            try
            {
                var parameters = new DynamicParameters();
                var reader = await _idbConnection.QueryMultipleAsync(_storedProcedureParams.Value.GetIwdPacs008IppcoreBatchAsync, parameters, commandTimeout: _serviceParams.Value.CommandTimeout, commandType: CommandType.StoredProcedure, transaction: null);
                if (reader != null)
                {
                    try
                    {
                        dbBatchPaymentParams = reader.Read<DBBatchPaymentParams>().ToList();
                        reader.Dispose();
                    }
                    catch(Exception ex)
                    {
                       _logger.Error(ex,"SqlData", "GetDataFromDatabaseAsync", $"Error occurred in GetDataFromDatabaseAsync(): {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"SqlData", "GetDataFromDatabaseAsync", $"Error occurred in GetDataFromDatabaseAsync(): {ex.Message}");
            }
            _logger.Info("SqlData", "GetDataFromDatabaseAsync", $"GetDataFromDatabaseAsync is done.");
            return dbBatchPaymentParams;
        }
        public async Task<DBParamsRoot> GetDataFromDatabaseMttoMx(Logger _logger)
        {
            var dbParamsRoot = new DBParamsRoot();
            _logger.Info("SqlData", "GetDataFromDatabaseAsync", $"Started.");
            try
            {
                var parameters = new DynamicParameters();
                var reader = await _idbConnection.QueryMultipleAsync(_storedProcedureParams.Value.GetIwdPacs008IppcoreBatchAsync!, parameters, commandType: CommandType.StoredProcedure, transaction: null);
                if (reader != null)
                {
                    try
                    {
                        dbParamsRoot.DBRequestHeader = reader.Read<DBRequestHeader>().FirstOrDefault();
                        dbParamsRoot.DBRequestDetails = reader.Read<DBRequestDetails>().ToList();
                        //dbParamsRoot.DBParams = reader.Read<DBParams>().FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "SqlData", "GetDataFromDatabaseAsync", $"Error occurred in GetDataFromDatabaseAsync(): {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper() == "NO COLUMNS WERE SELECTED")
                {
                    dbParamsRoot = null;
                    _logger.Error(ex, "SqlData", "GetDataFromDatabaseAsync", $"No record found to generate pacs008.");
                }
                else
                {
                    _logger.Error("SqlData", "GetDataFromDatabaseAsync", $"Error occurred in GetDataFromDatabaseAsync(): {ex.Message}");
                }
            }
            _logger.Info("SqlData", "GetDataFromDatabaseAsync", $"GetDataFromDatabaseAsync is Done.");
            return dbParamsRoot!;
        }
        public async Task UpdateBatchPaymentDetailsAsync(string refenceNbr, decimal srlNbr, string status, string headerstatus, string XsdPaymentStatus, string swiftMessage, string creditoriban,string EndToEndidentification, Logger _logger)
        {
            _logger.Info("SqlData", "UpdateBatchPaymentDetailsAsync", $"UpdateBatchPaymentDetailsAsync Invoked.");
            try
            {
                if (_idbConnection.State == ConnectionState.Closed)
                {
                    _idbConnection.Open();
                    _logger.Info("SqlData", "UpdateBatchPaymentDetailsAsync", $"DBConnection opened");
                }
                var parameters = new DynamicParameters();
                parameters.Add("BATDET_REF_ID", refenceNbr, DbType.String);
                parameters.Add("SRNO", srlNbr, DbType.String);
                parameters.Add("Message_Status", status, DbType.String);
                parameters.Add("Message_hdrstatus", headerstatus, DbType.String);
                parameters.Add("Swift_Message", swiftMessage, DbType.String);
                parameters.Add("creditoriban", creditoriban, DbType.String);
                parameters.Add("EndToEndidentification", EndToEndidentification, DbType.String);
                parameters.Add("XsdPaymentStatus", XsdPaymentStatus, DbType.String);

                _logger.Info("SqlData", "UpdateBatchPaymentDetailsAsync", $"UpdateBatchPaymentDetailsAsync Params.BATDET_REF_ID: {refenceNbr},SrlNbr: {srlNbr}, Message_Status: {status}, Message_hdrstatus:{headerstatus }, XsdPaymentStatus:{XsdPaymentStatus}");
                await _idbConnection.QueryAsync(_storedProcedureParams.Value.UpdateIwdPacs008IppcoreBatchDetailsAsync, parameters, commandTimeout: _serviceParams.Value.CommandTimeout, commandType: CommandType.StoredProcedure, transaction: null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"SqlData", "UpdateBatchPaymentDetailsAsync", $"Error occurred in UpdateBatchPaymentDetailsAsync(): {ex.Message}");
            }
            _logger.Info("SqlData", "UpdateBatchPaymentDetailsAsync", $"UpdateBatchPaymentDetailsAsync Done.");
        }

        public async Task UpdateValidationStatusAsync(string refenceNbr, decimal srlNbr, string status, string headerStatus,string iban, string errorMessage, Logger _logger)
        {
            _logger.Info("SqlData", "UpdateValidationStatusAsync", $"UpdateValidationStatusAsync Invoked.");
            try
            {
                if (_idbConnection.State == ConnectionState.Closed)
                {
                    _idbConnection.Open();
                    _logger.Info("SqlData", "UpdateValidationStatusAsync", $"DBConnection opened");
                }
                var parameters = new DynamicParameters();
                parameters.Add("Referenceid", refenceNbr, DbType.String);
                parameters.Add("Srno", srlNbr, DbType.String);
                parameters.Add("Message_Status", status, DbType.String);
                parameters.Add("Message_hdrstatus", headerStatus, DbType.String);
                parameters.Add("iban", iban, DbType.String);
                parameters.Add("ErrorMsg", errorMessage, DbType.String);
                _logger.Info("SqlData", "UpdateValidationStatusAsync", $"UpdateValidationStatusAsync Params.Referenceid: {refenceNbr},SrlNbr: {srlNbr}, Message_Status: {status}, Message_hdrstatus:{headerStatus}");
                await _idbConnection.QueryAsync(_storedProcedureParams.Value.UpdateIwdPacs008XsdValidationAsync, parameters, commandTimeout: _serviceParams.Value.CommandTimeout, commandType: CommandType.StoredProcedure, transaction: null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"SqlData", "UpdateValidationStatusAsync", $"Error occurred in UpdateValidationStatusAsync(): {ex.Message}");
            }
            _logger.Info("SqlData", "UpdateValidationStatusAsync", $"UpdateValidationStatusAsync Done.");
        }
    }
}
