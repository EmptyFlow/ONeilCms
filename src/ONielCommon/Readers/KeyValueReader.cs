namespace ONielCommon.Readers {

    public static class KeyValueReader {

        public static IDictionary<string, string> GetPairs ( string content ) {
            var result = new Dictionary<string, string> ();

            var lines = content.Split ( "\n" );

            foreach ( var line in lines ) {
                if ( !line.Contains ( " " ) ) continue;

                var span = line.AsSpan ();
                var index = span.IndexOf ( " " );

                var key = span.Slice ( 0, index );
                var value = span.Slice ( index );

                result.TryAdd ( key.ToString().Trim(), value.ToString () );
            }

            return result;
        }

    }

}
