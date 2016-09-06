using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public abstract class Response : OneOfBase<
        Response.MethodNotAllowed,
        Response.NotFoundResponse,
        Response.ModelBindingFailedResponse,
        Response.RepresentationResponse,
        Response.CreatedResponse
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
        public class CreatedResponse : Response
        {
            public string Description { get; set; }
            public Uri Uri { get; set; }

            public CreatedResponse(string description, Uri uri)
            {
                Description = description;
                Uri = uri;
            }
        }

    }
}