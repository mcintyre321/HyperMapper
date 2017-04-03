using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.ResourceModel;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Boards : Node<TaskListAppRoot>, IHasChildNodes
    {
        public Boards(TaskListAppRoot parent, UrlPart urlPart) : base(parent, urlPart, nameof(Boards), TermFactory.From<Boards>()) { }

        List<Board> _boards = new List<Board>();
        public IEnumerable<Board> Items => _boards;


        [Expose]
        public Board AddBoard(string title, string description, [Inject] IdGenerator guid)
        {
            var board = new Board(this, guid(), title) { Description = description };
            this._boards.Add(board);
            return board;
        }

        public override ChildNodes ChildNodes => base.ChildNodes.Concat(_boards);
    }
}