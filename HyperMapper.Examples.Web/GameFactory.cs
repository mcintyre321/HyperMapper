using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChessDotNet;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.Web
{
    public class GameFactory : RootNode
    {
        Dictionary<string, ChessDotNet.ChessGame> _games = new Dictionary<string, ChessGame>();
        public GameFactory(string title, Uri uri, Term term) : base(title, uri, term)
        {
        }

        [Expose]
        public ChessGameNode CreateGame()
        {
            var key = Guid.NewGuid().ToString();
            var chessGame = new ChessGame();
            _games.Add(key, chessGame);
            return new ChessGameNode(chessGame, this, new UrlPart(key));
        }

        public override AbstractNode GetChild(UrlPart key)
        {
            if (_games.ContainsKey(key.ToString()))
            {
                return new ChessGameNode(_games[key.ToString()], this, key);
            }
            return base.GetChild(key);
        }

        public override IEnumerable<UrlPart> ChildKeys => base.ChildKeys.Concat(_games.Keys.Select(k => new UrlPart(k)));
    }

    public class ChessGameNode : Node<GameFactory>
    {
        private readonly ChessGame _chessGame;

        public ChessGameNode(ChessGame chessGame, GameFactory parent, UrlPart gameId) :base(parent, gameId, "Game " + gameId, TermFactory.From<ChessGameNode>())
        {
            _chessGame = chessGame;
        }

        [Expose]
        public void MakeMove(MoveInfo moveInfo)
        {
            var move = _chessGame.Moves.Single(m =>
                (int) m.OriginalPosition.Rank == moveInfo.OrigX &&
                (int)m.OriginalPosition.File == moveInfo.OrigY &&
                (int)m.NewPosition.Rank == moveInfo.DestX &&
                (int)m.NewPosition.File == moveInfo.DestY
                );
            _chessGame.ApplyMove(move, false);
        }

        public IEnumerable<MoveInfo> MakeMove_choices()
        {
            return _chessGame.Moves.Select(m => new MoveInfo(m.OriginalPosition.Rank, (int) m.OriginalPosition.File, m.NewPosition.Rank, (int) m.NewPosition.File, m.Promotion));
        }
        
        
    }

    class Choice { }


    public class MoveInfo
    {
        public int OrigX { get; set; }
        public int OrigY { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
        public char? Promotion { get; set; }
        public MoveInfo() { }
        public MoveInfo(int origX, int origY, int destX, int destY, char? promotion)
        {
            OrigX = origX;
            OrigY = origY;
            DestX = destX;
            DestY = destY;
            Promotion = promotion;
        }

       
    }
}