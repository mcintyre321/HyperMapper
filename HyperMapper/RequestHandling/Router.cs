using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.RequestHandling
{
    public delegate OneOf<Resource<TRepresentation>, None> Router<TRepresentation>(BaseUrlRelativePath path);
}