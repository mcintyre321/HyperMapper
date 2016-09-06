using System;
using OneOf;

namespace HyperMapper.ResourceModel
{
    public class Method : OneOfBase<Method.Get, Method.Post>
    {
        public class Get : Method { }
        public class Post : Method { }

        public override bool Equals(object obj) => obj?.GetType() == this.GetType();
        public override int GetHashCode() => this.GetType().Name.GetHashCode();

        public static bool operator ==(Method left, Method right) => Equals(left, right);
        public static bool operator !=(Method left, Method right) => !Equals(left, right);

        public static Method Parse(string method)
        {
            switch (method.ToUpper())
            {
                case "POST":
                    return new Post();
                case "GET":
                    return new Get();
                default:
                    throw new NotImplementedException("Method not implemente3d" + method);
            }
        }
    }
}