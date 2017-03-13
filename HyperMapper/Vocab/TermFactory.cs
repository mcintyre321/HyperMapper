using System.Reflection;

namespace HyperMapper.Vocab
{
    public static class TermFactory
    {
        public static Term From(PropertyInfo propertyInfo)
        {
            return new Term(propertyInfo.DeclaringType.FullName + "_" + propertyInfo.Name);
        }

        public static Term From(MethodInfo methodInfo)
        {
            return  new Term(methodInfo.DeclaringType.FullName + "_" + methodInfo.Name)
            {
                Meanings = { TermFactory.From<Operation>()}
            };
        }

        public static Term From<T>()
        {
            return new Term(typeof(T).FullName);
        }

    }
}