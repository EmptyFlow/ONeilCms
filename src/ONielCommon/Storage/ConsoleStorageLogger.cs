namespace ONielCommon.Storage {

    public class ConsoleStorageLogger : IStorageLogger {

        public void LogInformation ( string message ) => Console.WriteLine ( message );

    }

}
