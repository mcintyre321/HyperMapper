using OneOf;

namespace HyperMapper.HyperModel
{
    public class InvokeResult : OneOfBase<InvokeResult.RepresentationResult>
    {
        public class RepresentationResult : InvokeResult
        {
            public Representation Representation { get; }

            public RepresentationResult(Representation representation)
            {
                Representation = representation;
            }
        }
    }
}