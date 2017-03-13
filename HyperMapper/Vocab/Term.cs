using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperMapper.Vocab
{
    public class Term
    {
        public List<Term> Meanings = new List<Term>();
        private string _value;

        protected internal Term(string name)
        {
            _value = name;
        }

        public string UrlPart => "_" + _value.Replace(".", "-");
        public string Title => _value;


        public bool Means(Term @from)
        {
            return this.Meanings.Contains(@from);
        }

        #region Generated Equality

        

        protected bool Equals(Term other)
        {
            return Meanings.SequenceEqual(other.Meanings) && string.Equals(_value, other._value);
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
            unchecked
            {

                return Meanings
                    .Select(m => m.GetHashCode()).Concat(new[] {_value?.GetHashCode() ?? 0})
                    .Aggregate(19, (hash, r) => hash * 31 + r.GetHashCode());
            }
        }

        public static bool operator ==(Term left, Term right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Term left, Term right)
        {
            return !Equals(left, right);
        }

        #endregion

    }
}