using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public abstract class Response<TRep> : OneOfBase<
        Response<TRep>.MethodNotAllowed,
        Response<TRep>.NotFoundResponse,
        Response<TRep>.ModelBindingFailedResponse,
        Response<TRep>.RepresentationResponse,
        Response<TRep>.CreatedResponse
        >
    {
        public class MethodNotAllowed : Response<TRep> { }
        public class NotFoundResponse : Response<TRep> { }
        public class ModelBindingFailedResponse : Response<TRep> { }
        public class RepresentationResponse : Response<TRep>
        {
            public TRep Representation { get; }

            internal RepresentationResponse(TRep representation)
            {
                Representation = representation;
            }

        }
        public class CreatedResponse : Response<TRep>
        {
            public TRep Representation { get; private set; }

            public CreatedResponse(TRep representation)
            {
                Representation = representation;
            }
        }

    }
}