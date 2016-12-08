using System.Reflection;

namespace HyperMapper.RepresentationModel
{
    public static class TermFactory
    {
        public static Term[] From(PropertyInfo propertyInfo)
        {
            return new [] { new Term(propertyInfo.DeclaringType.FullName + "_" + propertyInfo.Name)};
        }

        public static Term[] From(MethodInfo methodInfo)
        {
            return new[] { new Term(methodInfo.DeclaringType.FullName + "_" + methodInfo.Name)};
        }

        public static Term[] From<T>()
        {
            return new[] { new Term(typeof(T).FullName)};
        }

    }
}