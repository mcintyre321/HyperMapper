using System.Collections.Generic;
using HyperMapper;
using HyperMapper.Examples.TaskList.Domain.Ports;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Boards : Node<TaskListAppRoot>
    {
        public Boards(TaskListAppRoot parent, UrlPart urlPart) : base(parent, urlPart, nameof(Boards), TermFactory.From<Boards>()) { }

        List<Board> _boards = new List<Board>();
        [Expose]
        public IEnumerable<Board> Items => _boards;


        [Expose]
        public Board AddBoard(string title, string description, [Inject] IdGenerator guid)
        {
            var board = new Board(this, guid(), title) { Description = description };
            AddChild(board);
            this._boards.Add(board);
            return board;
        }

    }
}