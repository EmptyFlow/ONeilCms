namespace ONielCommon.Exceptions {

    public class StatusCodeException : Exception {

        public int StatusCode { get; set; }

        public StatusCodeException () : base () {
        }

        public StatusCodeException ( string message ) : base ( message ) {

        }

        public StatusCodeException ( string message, Exception innerException ) : base ( message, innerException ) {
        }

        public static StatusCodeException Create ( int statusCode ) {
            var exception = new StatusCodeException ( "Status code exception " + statusCode );
            exception.StatusCode = statusCode;

            return exception;
        }

    }

}
