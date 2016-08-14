using System.Threading.Tasks;

namespace HyperMapper.RequestHandling
{
    public delegate Task<Response> RequestHandler(Request request, BindModel readBody);
}