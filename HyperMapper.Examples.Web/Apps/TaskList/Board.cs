using System;
using System.Collections.Generic;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Board : Node<Boards>
    {
        public Board(Boards parent, string key, string title) : base(parent, key, title, TermFactory.From<Board>()) { }

        readonly List<Card> _cards = new List<Card>();

        public IEnumerable<Card> Cards => _cards;

        public override ChildNodes ChildNodes => base.ChildNodes.Concat(_cards);

        [Expose]
        public string Description { get; set; }
        

        [Expose]
        public void AddCard(string title, string description, [Inject] IdGenerator guid)
        {
            var item = new Card(this, guid(), title) { Description = description };
            this._cards.Add(item);
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