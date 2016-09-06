namespace HyperMapper.ResourceModel
{
    public class ObjectResult
    {
        public object Result { get; }

        public ObjectResult(object result)
        {
            Result = result;
        }
    }
}