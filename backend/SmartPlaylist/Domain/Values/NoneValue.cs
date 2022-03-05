namespace SmartPlaylist.Domain.Values
{
    public sealed class NoneValue : EmptableValue
    {
        public override string Kind { get; } = nameof(NoneValue);
        internal override bool IsEmpty => true;
    }
}