using HyperMapper.Model;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public abstract class Response : OneOfBase<
        Response.MethodNotAllowed,
        Response.NotFoundResponse,
        Response.ModelBindingFailedResponse,
        Response.RepresentationResponse
        >
    {
        public class MethodNotAllowed : Response { }
        public class NotFoundResponse : Response { }
        public class ModelBindingFailedResponse : Response { }
        public class RepresentationResponse : Response
        {
            public Representation Representation { get; }

            internal RepresentationResponse(Representation representation)
            {
                Representation = representation;
            }

        }
    }
}