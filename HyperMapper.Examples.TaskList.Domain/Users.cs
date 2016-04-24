using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HyperMapper;
using HyperMapper.DomainMapping;
using HyperMapper.Examples.TaskList.Domain.Ports;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Users : Node<AppRoot>
    {
        List<User> _users = new List<User>();
        public Users(AppRoot parent, Key key) : base(parent, key) { }
        [Expose]
        public void Register(string username, [Inject] IdGenerator idGenerator)
        {
            var user = new User(this, idGenerator())
            {
                Username = username
            };
            _users.Add(user);
        }

        [Expose]
        public OneOf.OneOf<OK, UserError> SignIn(string username, [Inject] CurrentUserSetter currentUserSetter)
        {
            var user = _users.SingleOrDefault(x => x.Username == username);
            if (user == null) return new UserError("username not found");
            currentUserSetter(user.Key.ToString());
            return new OK();
        }
    }
}