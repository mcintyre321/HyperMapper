using System;
using System.Collections.Generic;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapping;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Board : Node<Boards>
    {
        public Board(Boards parent, string key) : base(parent, key) { }

        List<Card> _cards = new List<Card>();
        public IEnumerable<Card> Cards => _cards;

        [Expose]
        public string Title { get; set; }

        [Expose]
        public void AddCard(string description, [Inject] IdGenerator guid)
        {
            var item = new Card(this, guid()) { Description = description };
            this._cards.Add(item);
            this.AddChild(item);

        }

        [Expose]
        public void Delete()
        {
            this.Deleted = true;
        }
        [Expose]
        public bool Deleted { get; set; }
    }
}