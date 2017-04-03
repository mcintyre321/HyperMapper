using System;
using System.Collections.Generic;
using System.Linq;
using ChessDotNet;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.Mapping.ResultTypes;
using HyperMapper.Vocab;

namespace HyperMapper.Examples.Web.Apps
{
    public class ChessEngineApp : RootNode
    {
        Dictionary<string, ChessDotNet.ChessGame> _games = new Dictionary<string, ChessGame>();
        public ChessEngineApp(string title, Uri uri, Term term) : base(title, uri, term)
        {
        }

        [Expose]
        public Created CreateGame()
        {
            var key = Guid.NewGuid().ToString();
            var chessGame = new ChessGame();
            _games.Add(key, chessGame);
            return new Created(new ChessGameNode(chessGame, this, new UrlPart(key)));
        }
    }

    public class ChessGameNode : Node<ChessEngineApp>
    {
        private readonly ChessGame _chessGame;

        public ChessGameNode(ChessGame chessGame, ChessEngineApp parent, UrlPart gameId) :base(parent, gameId, "Game " + gameId, TermFactory.From<ChessGameNode>())
        {
            _chessGame = chessGame;
        }

        [Expose]
        public BoardVm Board
        {
            get
            {
                var squares = this._chessGame.GetBoard()
                    .SelectMany(p => p)
                    .Select(p => new BoardVm.SquareView(p?.GetFenCharacter(), p?.Owner.ToString()))
                    .ToArray();
                return new BoardVm(squares, 8, 8);
            }
        }


        [Expose]
        public Modified MakeMove([OptionsFrom(nameof(MakeMove_options))]MoveInfo moveInfo)
        {
            var move = _chessGame.GetValidMoves(_chessGame.WhoseTurn).Single(m =>
                (int) m.OriginalPosition.Rank == moveInfo.OrigX &&
                (int)m.OriginalPosition.File == moveInfo.OrigY &&
                (int)m.NewPosition.Rank == moveInfo.DestX &&
                (int)m.NewPosition.File == moveInfo.DestY &&
                (char?)m.Promotion == moveInfo.Promotion &&
                m.Player.ToString() == moveInfo.Player
                );
            _chessGame.ApplyMove(move, false);
            return new Modified(this);
        }

        public IEnumerable<MoveInfo> MakeMove_options()
        {
            return _chessGame
                .GetValidMoves(_chessGame.WhoseTurn)
                .Select(m => new MoveInfo(m.Player.ToString(), m.OriginalPosition.Rank, (int) m.OriginalPosition.File, m.NewPosition.Rank, (int) m.NewPosition.File, m.Promotion));
        }
    }

    public class BoardVm
    {
        public IReadOnlyCollection<SquareView> Squares { get; }
        public int Rows { get; }
        public int Cols { get; }

        public BoardVm(SquareView[] squares, int rows, int cols)
        {
            Squares = squares;
            Rows = rows;
            Cols = cols;
        }

        public class SquareView
        {
            public char? Char { get; }
            public string Color { get; }

            public SquareView(char? @char, string color)
            {
                Char = @char;
                Color = color;
            }
        }
    }


    public class MoveInfo
    {
        public int OrigX { get; set; }
        public int OrigY { get; set; }
        public int DestX { get; set; }
        public int DestY { get; set; }
        public char? Promotion { get; set; }
        public string Player { get; set; }

        public MoveInfo() { }
        public MoveInfo(string player, int origX, int origY, int destX, int destY, char? promotion)
        {
            Player = player;
            OrigX = origX;
            OrigY = origY;
            DestX = destX;
            DestY = destY;
            Promotion = promotion;
        }

    }
}