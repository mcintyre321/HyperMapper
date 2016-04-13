using HyperMapper;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class User : Node<Users>
    {
        public string Username { get; internal set; }
        public User(Users parent, Key key) : base(parent, key) { }

    }
}