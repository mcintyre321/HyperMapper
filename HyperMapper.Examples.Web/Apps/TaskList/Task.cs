using HyperMapper.Mapping;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.TaskList.Domain
{
    class Task : Node<Card>
    {
        public Task(Card parent, UrlPart urlPart, string title) : base(parent, urlPart, title, TermFactory.From<Task>()) { }
        [Expose]
        public string Description { get; set; }
        [Expose]
        public bool Done { get; [Expose] set; }

    }
}