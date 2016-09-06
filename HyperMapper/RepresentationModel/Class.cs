using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperMapper.RepresentationModel
{
    public class Class
    {
        private readonly string _value;
        public Class(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected bool Equals(Class other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Class) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static bool operator ==(Class left, Class right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Class left, Class right)
        {
            return !Equals(left, right);
        }
    }
}
