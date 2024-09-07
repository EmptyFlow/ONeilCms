namespace ONielCommon.Readers {

    public static class KeyValueReader {

        public static IDictionary<string, string> GetPairs ( string content ) {
            var result = new Dictionary<string, string> ();

            var lines = content.Split ( "\n" );

            foreach ( var line in lines ) {
                if ( !line.Contains ( " " ) ) continue;

                var parts = line.Split ( " " );
                if ( parts.Count () != 2 ) continue;

                var key = parts[0].Trim ();
                var value = parts[1].Trim ();

                result.TryAdd ( key, value );
            }

            return result;
        }

    }

}
