namespace WDLT.PoESpy.Models
{
    public class AppItemProperty
    {
        public string Text { get; }
        public bool IsValue { get; }
        public int? ValueType { get; }

        public AppItemProperty(string text, bool isValue, int? valueType = null)
        {
            Text = text.Trim();
            IsValue = isValue;
            ValueType = valueType;
        }
    }
}