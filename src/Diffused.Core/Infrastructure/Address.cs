using System;

namespace Diffused.Core.Infrastructure
{
    public class Address:IEquatable<Address>
    {
        public Address(string value)
        {
            Value = value;
            Key = Value;
        }

        public string Key { get; }
        
        public string Value { get; }

        public override string ToString()
        {
            return $"inmem:{Value}";
        }

        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}