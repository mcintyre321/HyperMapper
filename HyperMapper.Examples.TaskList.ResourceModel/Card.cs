using System.Collections.Generic;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Card : Node<Board>
    {
        List<Task> _tasks = new List<Task>();
        [Expose]
        public string Description { get; set; }
        public Card(Board parent, UrlPart urlPart, string title) : base(parent, urlPart, title, TermFactory.From<Card>()) { }

        [Expose]
        public void AddTask(string title, string description, [Inject] IdGenerator idGenerator)
        {
            this._tasks.Add(new Task(this, new UrlPart(idGenerator()), title)
            {
                Description = description
            });
        }
    }
}