using HyperMapper;
using HyperMapper.Mapping;

namespace HyperMapper.Examples.TaskList.Domain
{
    class Task : Node<Card>
    {
        public Task(Card parent, Key key, string title) : base(parent, key, title) { }
        [Expose]
        public string Description { get; set; }
    }
}