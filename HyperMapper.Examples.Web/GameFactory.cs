using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChessEngine;
using HyperMapper.Mapper;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Examples.Web
{
    public class GameFactory : RootNode
    {
        Dictionary<string, ChessEngine.ChessGame> _games = new Dictionary<string, ChessGame>();
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

        public override INode GetChild(UrlPart key)
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
                m.Piece.Square.X == moveInfo.OrigX &&
                m.Piece.Square.Y == moveInfo.OrigY &&
                m.Destination.X == moveInfo.DestX &&
                m.Destination.Y == moveInfo.DestY
                );
            move.Apply();
        }

        public IEnumerable<MoveInfo> MakeMove_choices()
        {
            return _chessGame.Moves.Select(m => new MoveInfo(m.Piece.Square.X, m.Piece.Square.Y, m.Destination.X, m.Destination.Y));
        }
        
        
    }

    class Choice { }


    public class MoveInfo
    {
        public int OrigX { get; set; }
        public int OrigY { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
        public MoveInfo() { }
        public MoveInfo(int origX, int origY, int destX, int destY)
        {
            OrigX = origX;
            OrigY = origY;
            DestX = destX;
            DestY = destY;
        }

       
    }
}