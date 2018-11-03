namespace CodeAnalyzers.AdditionalRules
{
    using System.Runtime.Serialization;

    [DataContract]
    class AnalyzerSettings
    {
        [DataMember]
        public Settings settings { get; set; }
    }

    [DataContract]
    class Settings
    {

        [DataMember]
        public ReadabilityRules readabilityRules { get; set; }
    }

    [DataContract]
    class ReadabilityRules
    {
        [DataMember]
        public int maximumLineLength { get; set; }
    }
}
