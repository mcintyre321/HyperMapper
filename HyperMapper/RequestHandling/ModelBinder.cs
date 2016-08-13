using System;
using System.Threading.Tasks;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public delegate Task<OneOf<ModelBindingFailed, MethodParameters>> ModelBinder(Tuple<Key, Type>[] keys);
}