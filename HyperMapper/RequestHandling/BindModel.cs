using System;
using System.Threading.Tasks;
using HyperMapper.ResourceModel;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public delegate Task<OneOf<ModelBindingFailed, MethodArguments>> BindModel(MethodParameter[] methodParameters);
}