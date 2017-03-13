using OneOf;

namespace HyperMapper.ResourceModel
{
    public class InvokeResult<TRep> : OneOfBase<InvokeResult<TRep>.RepresentationResult>
    {
        public class RepresentationResult : InvokeResult<TRep>
        {
            public TRep Representation { get; }

            public RepresentationResult(TRep representation)
            {
                Representation = representation;
            }
        }
    }
}