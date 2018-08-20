namespace PawnRules.Data
{
    internal interface IPresetableType
    {
        string Id { get; }
        string Label { get; }
        string LabelPlural { get; }
    }
}
