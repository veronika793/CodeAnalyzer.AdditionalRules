namespace CodeAnalyzers.AdditionalRules
{
    using System.Runtime.Serialization;

    [DataContract]
    class AnalyzerSettings
    {
        [DataMember(Name = "settings")]
        public Settings Settings { get; set; }
    }

    [DataContract]
    class Settings
    {

        [DataMember(Name = "readabilityRules")]
        public ReadabilityRules ReadabilityRules { get; set; }
    }

    [DataContract]
    class ReadabilityRules
    {
        [DataMember(Name = "maximumLineLength")]
        public int MaximumLineLength { get; set; }
    }
}
