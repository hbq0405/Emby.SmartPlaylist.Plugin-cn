using SmartPlaylist.Domain.Values;

namespace SmartPlaylist.Domain.Rule
{
    public class RuleCriteriaValue
    {
        public RuleCriteriaValue(Value value, Operator.Operator @operator,
            CriteriaDefinition.CriteriaDefinition criteriaDefinition)
        {
            Value = value;
            Operator = @operator;
            CriteriaDefinition = criteriaDefinition;
        }

        public Value Value { get; }

        public Operator.Operator Operator { get; }

        public CriteriaDefinition.CriteriaDefinition CriteriaDefinition { get; }

        public bool IsMatch(UserItem item)
        {
            var itemValue = CriteriaDefinition.GetValue(item);
            if (Operator.CanCompare(itemValue, Value))
            {
                bool result = Operator.Compare(itemValue, Value);
                item.SmartPlaylist.Log($"Comparing:[{itemValue.Kind.ToString()}] {item.Item.Name} - {CriteriaDefinition.Name}: '{itemValue.Friendly}' {Operator.Name} '{Value.Friendly}' = {result}");
                return result;
            }
            else
            {
                item.SmartPlaylist.Log(
                    itemValue.IsNone ?
                    $"NOT Comparing: {item.Item.Name} - {CriteriaDefinition.Name}: 'No metadata value found'" :
                    $"NOT Comparing: {item.Item.Name} - {CriteriaDefinition.Name}: '{itemValue.Friendly}' {Operator.Name} '{Value.Friendly}'");
                return false;
            }
        }
    }
}