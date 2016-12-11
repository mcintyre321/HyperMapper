using HyperMapper;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    class Task : Node<Card>
    {
        public Task(Card parent, UrlPart urlPart, string title) : base(parent, urlPart, title, TermFactory.From<Task>()) { }
        [Expose]
        public string Description { get; set; }
    }
}