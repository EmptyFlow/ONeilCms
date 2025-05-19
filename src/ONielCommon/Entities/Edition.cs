namespace ONielCommon.Entities {

    public class Edition {

        public Guid Id { get; init; }

        public string Version { get; init; } = "";

        public DateTime Created { get; set; }

    }

}
