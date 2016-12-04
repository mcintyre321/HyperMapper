using HyperMapper;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    class Task : Node<Card>
    {
        public Task(Card parent, Key key, string title) : base(parent, key, title, TermFactory.From<Task>()) { }
        [Expose]
        public string Description { get; set; }
    }
}