using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.Mapper.ActionResults;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Authentication : Node<AppRoot>
    {
        public Authentication(AppRoot parent, UrlPart urlPart) : base(parent, urlPart, nameof(Authentication), TermFactory.From<Authentication>()) { }
        [Expose]
        public OneOf.OneOf<OK, UserError> Register(string username, string password)
        {
            return new OK();
        }

        [Expose]
        public OneOf.OneOf<OK, UserError> SignIn(string username, [Inject] CurrentUserSetter currentUserSetter)
        {
            return new OK();
        }
    }
}