using System;

namespace HyperMapper.RepresentationModel
{
    public sealed class Term
    {
        protected bool Equals(Term other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Term) obj);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Term left, Term right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Term left, Term right)
        {
            return !Equals(left, right);
        }

        private string _value;

        protected internal Term(string name)
        {
            _value = name;
        }

        public override string ToString()
        {
            return _value;
        }

        public static Term Title => new Term(nameof(Title));
        public static Term Parent => new Term(nameof(Parent));
        public static Term Child => new Term(nameof(Child));


    }
}