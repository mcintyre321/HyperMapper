namespace HyperMapper.Mapping
{
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class RootNode : Node
    {
        public RootNode(string title) : base(title)
        {
        }
    }
}
