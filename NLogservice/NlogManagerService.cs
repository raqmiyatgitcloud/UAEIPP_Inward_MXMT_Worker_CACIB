using NLog;

namespace NLogFluent
{
    public class NLogMXtoMTConversionWorker
    {
        public readonly NLog.Logger _log = LogManager.GetLogger("NLogMXtoMTConversionWorker");
        public void Debug(string ClassName, string MethodName, string Message)
        {
            string loggingMessage = ForStructuredLog(ClassName, MethodName, Message);
            _log.Debug(loggingMessage);
        }
        public void Error(string ClassName, string MethodName, string Message)
        {
            string loggingMessage = ForStructuredLog(ClassName, MethodName, Message);
            _log.Error(loggingMessage);
        }
        public void Info(string ClassName, string MethodName, string Message)
        {
            string loggingMessage = ForStructuredLog(ClassName, MethodName, Message);
            _log.Info(loggingMessage);
        }
        public void Warn(string ClassName, string MethodName, string Message)
        {
            string loggingMessage = ForStructuredLog(ClassName, MethodName, Message);
            _log.Warn(loggingMessage);
        }
        private string ForStructuredLog(string ClassName, string MethodName, string Message)
        {
            return $"-----------------------------------------------\r\n Class Name :{ClassName}.\r\n Method Name : {MethodName} \r\n Message : {Message}. \r\n ----------------------------------------------------------------------------";
        }

    }

}
