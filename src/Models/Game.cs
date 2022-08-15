
using Chess.Interfaces;
using Mindmagma.Curses;
using System.Text.Json;

namespace Chess.Models;


public class Game : IChessGame
{
    private IntPtr _win;

    public string? Cursor { get; private set; } = null;

    public string? PieceSelectedAt { get; private set; }
    public string? PieceReleasedAt { get; private set; }

    private int _activeColor = 0;
    private int _playerColor;

    private bool _withAi = false;

    private int _presentation = 1;

    public string? Checked { get; private set; } = null;

    public bool[] Castled { get; } = { false, false };


    public Piece? Promotee { get; set; }

    private Board _board;
    public bool IsPlaying { get; private set; }

    public List<Action> Actions { get; private set; } = new List<Action> { };
    public Game()
    {
        _board = NewBoard();
        Setup();
    }

    public void Setup()
    {
        IsPlaying = true;
        _playerColor = 0;
        Actions = new List<Action> { };
        _activeColor = 0;
        _win = NCurses.InitScreen();
        NCurses.NoDelay(_win, true);
        NCurses.NoEcho();
        NCurses.Keypad(_win, true);
        NCurses.StartColor();
    }


    private Board NewBoard()
    {
        {
            using (StreamReader r = new StreamReader("start.json"))
            {
                return new Board(r.ReadToEnd());
            }
        }
    }

    public void LoadGame(string fileName)
    {
        {
            using (StreamReader r = new StreamReader($"savegames/{fileName}.json"))
            {
                var saveGame = JsonSerializer.Deserialize<SaveGame>(r.ReadToEnd(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                _board = new Board(JsonSerializer.Serialize(saveGame.Squares));
                _activeColor = saveGame.ActiveColor;
                Actions = saveGame.Actions;
            }
        }
    }

    public void SaveGame(string fileName)
    {
        var saveGame = new SaveGame(_board.Squares, Actions, _activeColor);
        File.WriteAllText($"savegames/{fileName}.json", JsonSerializer.Serialize(saveGame));

    }

    public void SetCursor(string key)
    {

    }


    public void SelectPiece()
    {
        // var letter = "abcdefgh";
        // PieceSelectedAt = $"{letter[CursorX]}{CursorY + 1}";
    }

    public void ReleasePiece()
    {
        //     var letter = "abcdefgh";
        //     PieceReleasedAt = $"{letter[CursorX]}{CursorY + 1}";

        //     // piece not moved
        //     if (PieceReleasedAt != PieceSelectedAt)
        //     {
        //         MakeMove($"{PieceSelectedAt}-{PieceReleasedAt}");
        //     }
        //     PieceReleasedAt = null;
        //     PieceSelectedAt = null;
    }

    public void UndoAction()
    {
        if (Actions.Count == 0)
        {
            throw new Exception("no moves found");
        }
        // revert last move
        var lastAction = Actions.Last();
        // if (lastTurn.castling.HasValue)
        // {
        //     RevertCastling();
        //     return;
        // }
        if (lastAction.Move is not null)
        {
            var parsed = ParseMove(lastAction.Move);
            parsed.Revert();
            _board.MakeMove(parsed.From, parsed.To, parsed.From.Piece!);
        }
        if (lastAction.Capture is not null)
        {
            // get the square at which the piece was captured
            var square = _board.GetSquareByAddress(lastAction.Capture.Address);

            // re-populate square with piece
            square.Update(lastAction.Capture.Piece);
        }
        Actions.RemoveAt(Actions.Count() - 1);
        SwitchTurns();

    }

    // parse string presentation 'A1:A2' to move
    public IChessMove ParseMove(string move)
    {
        if (move.Length != 5)
        {
            throw new MoveParseError();
        }
        string[] addresses = move.Split("-");
        if (addresses.Length != 2)
        {
            throw new MoveParseError();
        }
        return new Move(_board.GetSquareByAddress(addresses[0]), _board.GetSquareByAddress(addresses[1]));
    }

    public void PrintBoard(string? msg)
    {
        NCurses.ClearWindow(_win);
        _board.PrintBoard(_win, Cursor, msg);
        NCurses.Refresh();
    }

    public string PrintTurns()
    {
        var s = "\nmoves:\n";
        string? capture = null;
        foreach (var action in Actions)
        {
            capture = action.Capture is not null ? $"x{action.Capture.Piece.Type}" : null;
            s += $"{(action.Piece.Color == 0 ? "w" : "b")}{action.Piece.Type} {action.Move}{capture}\n";
        }
        return s;
    }

    public void SwitchTurns()
    {
        _activeColor = 1 - _activeColor;
        if (!_withAi)
        {
            _playerColor = 1 - _playerColor;
        }
    }

    public void MakeMoveAi()
    {
        // computer move

    }

    private void RevertCastling()
    {
        // var lastMove = Events.Last();
        // var color = lastMove.Piece.Color;
        // var castlingMoves = from e in Events
        //                     where e.Piece.Color == color && e.castling.HasValue
        //                     select e;
        // var kingStartingSquare = _board.GetSquareByAddress(color == 0 ? "e1" : "e8");
        // var kingCurrentSquare = _board.GetSquareByAddress(_board.Kings[color].Address);
        // _board.MakeMove(kingCurrentSquare, kingStartingSquare, kingCurrentSquare.Piece!);
    }

    // attempt castling of rook at address
    public void Castle(string address)
    {
        if (Castled[_activeColor])
        {
            throw new Exception("Already castled. Each side may only castle once.");
        }
        var rookSquare = _board.GetSquareByAddress(address);
        var rook = rookSquare.Piece;
        if (rook?.Type != PieceType.R)
        {
            throw new Exception("Castling only allowed with R");
        }
        var rookMoves = from e in Actions where e.Piece.Id == rook.Id select e;
        if (rookMoves.Count() > 0)
        {
            throw new Exception("R has already moved. Castling not allowed.");
        }

        var kingSquare = _board.GetSquareByAddress(_board.Kings[_activeColor].Address);
        var king = kingSquare.Piece!;
        var kingMoves = from e in Actions where e.Piece.Id == king.Id select e;
        if (kingMoves.Count() > 0)
        {
            throw new Exception("K has already moved. Castling not allowed.");
        }
        var direction = kingSquare.File - rookSquare.File < 0 ? 1 : -1;

        // basic validation
        var rookMove = new Move(rookSquare, _board.Squares[kingSquare.Rank][kingSquare.File + 1 * direction]);
        _board.ValidateMove(rookMove, _activeColor);

        if (rookSquare.Rank != kingSquare.Rank)
        {
            throw new Exception("Castling only allowed on same rank");
        }
        string newAddress;
        Square? to;
        var steps = Enumerable.Range(1, 2);
        // move king
        foreach (var step in steps)
        {
            var from = _board.Squares[kingSquare.Rank][kingSquare.File + (step - 1) * direction];
            to = _board.Squares[kingSquare.Rank][kingSquare.File + step * direction];
            newAddress = $"{from.Address}-{to.Address}";
            _board.MakeMove(from, to, king);
            Actions.Add(new Action(king, $"{from.Address}-{to.Address}", null, null, true));
            var check = _board.EvaluateCheck();
            if (check.HasValue)
            {
                if (check.Value.color == _activeColor)
                {
                    foreach (var i in Enumerable.Range(0, step))
                    {
                        RevertCastling();
                    }
                    throw new Exception($"Castling under check not allowed. K checked by {check.Value.square.Piece!.Type} at {check.Value.square.Address} ");
                }
                _board.Kings[_activeColor].Address = to.Address;
            }

        }
        // move rook
        to = _board.Squares[kingSquare.Rank][kingSquare.File + 1 * direction];
        _board.MakeMove(rookSquare, to, rook);
        newAddress = $"{rookSquare.Address}-{to.Address}";
        Actions.Add(new Action(rook, $"{rookSquare.Address}-{to.Address}", null, null, true));

        Castled[_activeColor] = true;
    }

    // check if king at either side is checked
    // if active side king is checked the move is reverted.
    public void DetectCheck()
    {
        var check = _board.EvaluateCheck();
        if (!check.HasValue)
        {
            // neither side is checked. No need to take the analysis any further.
            return;
        }
        // the square the has the piece that exerts the check
        var offender = check.Value.square;

        // the side that is checked
        var color = check.Value.color;
        if (color == _activeColor)
        {
            UndoAction();
            SwitchTurns();
            throw new CheckError(color, offender.Address, offender.Piece!.Type);
        }
    }

    public void DetectPromotion(IChessMove move)
    {
        var piece = move.To.Piece;
        var rank = move.To.Rank;
        if (piece is null)
        {
            return;
        }
        if (piece.Type != PieceType.P)
        {
            return;
        }
        if (rank != 7 && rank != 0)
        {
            return;
        }
        if (piece.Color == 0 && rank == 7)
        {
            // add white pawn for promotion
            Promotee = piece;
        }
        if (piece.Color == 1 && rank == 0)
        {
            // add black pawn for promotion
            Promotee = piece;
        }
    }

    public void MakeMove(string move)
    {

        IChessMove? parsed = null;

        Capture? capture = null;
        parsed = ParseMove(move);
        var piece = parsed.From.Piece!;
        try
        {
            _board.ValidateMove(parsed, _activeColor);
            if (parsed.To.Piece is not null)
            {
                // capture at destination square
                capture = new Capture(parsed.To.Piece, parsed.To.Address);

            }
        }

        catch (CollisionError e)
        {
            var blocker = e.Square.Piece!;
            // en passant move.
            if (e.Mover.Type == PieceType.P)
            {
                // capture in passing
                capture = new Capture(blocker, e.Square.Address);
                e.Square.Update(null);
            }
            // castling
            if (e.Mover.Type == PieceType.R && blocker.Type == PieceType.K)
            {
                // ..
            }
            throw new Exception(e.Message);
        }
        _board.MakeMove(parsed.From, parsed.To, parsed.From.Piece!);
        Actions.Add(new Action(piece, move, capture, null, false));

        DetectCheck();

        DetectPromotion(parsed);
    }

    public void Quit()
    {
        IsPlaying = false;
    }
}


public record struct SaveGame(Square[][] Squares, List<Action> Actions, int ActiveColor) { }



