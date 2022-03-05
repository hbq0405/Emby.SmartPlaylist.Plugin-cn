namespace SmartPlaylist.Domain.Values
{
    public class ArrayValue<TValue> : EmptableValue where TValue : Value
    {
        private ArrayValue(TValue[] values)
        {
            Values = values ?? new TValue[0];
        }

        public TValue[] Values { get; }

        public override string Kind => "array";

        internal override bool IsEmpty => IsNone || Values.Length == 0;

        public static ArrayValue<TValue> Create(TValue[] values)

        {
            return new ArrayValue<TValue>(values);
        }
    }
}