using System;
using System.Threading.Tasks;
using HyperMapper.Model;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public delegate Task<OneOf<ModelBindingFailed, MethodArguments>> BindModel(MethodParameter[] methodParameters);
}