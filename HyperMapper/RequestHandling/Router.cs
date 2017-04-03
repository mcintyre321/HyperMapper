using System.Threading.Tasks;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.RequestHandling
{
    public delegate Task<OneOf<Resource<TRepresentation>, None>> Router<TRepresentation>(BaseUrlRelativePath path);
}