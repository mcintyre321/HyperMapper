using System;
using Newtonsoft.Json;

namespace HyperMapper
{
    [JsonConverter(typeof(Key.JsonConverter))]
    public class Key  
    {
        readonly string _value;

        public Key(string value)
        {
            this._value = value;
        }

        public override string ToString()
        {
            return _value;
        }
        public static implicit operator Key(string key)
        {
            return new Key(key);
        }

        public class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(value == null ? "null" : "\"" + value.ToString() + "\"");
            }

            public override bool CanConvert(Type objectType)
            {
                throw new System.NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return (Key) reader.ReadAsString();
            }
        }

        protected bool Equals(Key other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Key) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static bool operator ==(Key left, Key right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Key left, Key right)
        {
            return !Equals(left, right);
        }
    }
}