using System;
using System.Collections.Generic;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Board : Node<Boards>
    {
        public Board(Boards parent, string key, string title) : base(parent, key, title, TermFactory.From<Board>()) { }

        List<Card> _cards = new List<Card>();
        public IEnumerable<Card> Cards => _cards;

        [Expose]
        public string Description { get; set; }
        

        [Expose]
        public void AddCard(string title, string description, [Inject] IdGenerator guid)
        {
            var item = new Card(this, guid(), title) { Description = description };
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