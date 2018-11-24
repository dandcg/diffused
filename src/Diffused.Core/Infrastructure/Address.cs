namespace Diffused.Core.Infrastructure
{
    public class Address
    {
        public Address(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"inmem:{Value}";
        }
    }
}