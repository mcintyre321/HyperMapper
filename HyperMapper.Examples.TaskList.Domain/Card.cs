using System.Collections.Generic;
using HyperMapper;
using HyperMapper.DomainMapping;
using HyperMapper.Examples.TaskList.Domain.Ports;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Card : Node<Board>
    {
        List<Task> _tasks = new List<Task>();
        [Expose]
        public string Description { get; set; }
        public Card(Board parent, Key key) : base(parent, key) { }

        [Expose]
        public void AddTask(string description, [Inject] IdGenerator idGenerator)
        {
            this._tasks.Add(new Task(this, new Key(idGenerator()))
            {
                Description = description
            });
        }
    }
}