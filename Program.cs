using NLog;
using NLogFluent;
using Raqmiyat.Framework.Custom;
using Raqmiyat.Framework.Domain;
using Raqmiyat.Framework.Model;
using System.Data;
using System.Data.SqlClient;
using UAEIPP_Inward_MXMT_Worker.Model;

namespace UAEIPP_Inward_MXMT_Worker
{
    public static class Program
    {
        private static readonly Logger _logger = LogManager.GetLogger("ServiceLog");
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseWindowsService()

        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<NLogMXtoMTConversionWorker>();
            services.AddSingleton<ConnectCustom>();
            services.AddSingleton<Utils>();
            services.AddSingleton<Conversion>();
            services.AddSingleton<SqlData>();
           


            services = GetConfigurationSection(hostContext, services);
            services = GetSingletonIDbConnection(hostContext, services);
            services.AddHostedService<MXtoMTConversionWorker>();
        });

        public static IServiceCollection GetConfigurationSection(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<ServiceParams>(hostContext.Configuration.GetSection("ServiceParams"));
            services.Configure<DataBaseConnectionParams>(hostContext.Configuration.GetSection("DataBaseConnectionParams"));
            services.Configure<StoredProcedureParams>(hostContext.Configuration.GetSection("StoredProcedureParams"));
            services.Configure<FolderPath>(hostContext.Configuration.GetSection("FolderPath"));

            return services;
        }
        public static IServiceCollection GetSingletonIDbConnection(HostBuilderContext hostContext, IServiceCollection services)
        {

            services.AddSingleton<IDbConnection>(provider =>
            {
                var connectionStrings = hostContext.Configuration.GetSection(nameof(DataBaseConnectionParams)).Get<DataBaseConnectionParams>();
                IDbConnection dbConnection = new SqlConnection(SqlConManager.GetConnectionString(connectionStrings!.DBConnection!, connectionStrings.IsEncrypted));
                if (dbConnection.State != ConnectionState.Closed)
                {
                    _logger.Info(ForStructuredLog("Program", "GetSingletonIDbConnection", "DBConnection is already opened."));
                    return dbConnection;
                }
                dbConnection.Open();
                _logger.Info(ForStructuredLog("Program", "GetSingletonIDbConnection", "DBConnection opened"));
                return dbConnection;
            });
            return services;
        }
        private static string ForStructuredLog(string ControllerName, string MethodName, string Message)
        {
            return $"-----------------------------------------------\r\n class Name :{ControllerName}.\r\n Method Name : {MethodName} \r\n Message : {Message}. \r\n ----------------------------------------------------------------------------";
        }
    }
}