using System.Threading.Tasks;

namespace HyperMapper.RequestHandling
{
    public delegate Task<Response<TRep>> RequestHandler<TRep>(Request request, BindModel readBody);
}