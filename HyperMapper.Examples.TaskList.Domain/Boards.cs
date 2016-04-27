using System.Collections.Generic;
using HyperMapper;
using HyperMapper.DomainMapping;
using HyperMapper.Examples.TaskList.Domain.Ports;

namespace HyperMapper.Examples.TaskList.Domain
{
    public class Boards : Node<AppRoot>
    {
        public Boards(AppRoot parent, Key key) : base(parent, key) { }

        List<Board> _boards = new List<Board>();
        public IEnumerable<Board> Items => _boards;


        [Expose]
        public void AddBoard(string description, [Inject] IdGenerator guid)
        {
            var board = new Board(this, guid()) { Title = description };
            AddChild(board);
            this._boards.Add(board);
        }

    }
}