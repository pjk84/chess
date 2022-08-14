
using Chess.Interfaces;
using System.Linq;
using System.Text.Json;

namespace Chess.Models;


public class Game : IChessGame
{

    private int _activeColor = 0;
    private int _playerColor;

    private bool _withAi = false;

    private int _presentation = 1;

    public string? Checked { get; private set; } = null;

    public bool[] Castled { get; } = { false, false };

    public IChessPiece? Promotee { get; private set; }

    private Board _board;
    public bool IsPlaying { get; private set; }

    public List<Turn> Turns { get; private set; } = new List<Turn> { };
    public Game()
    {
        _board = NewBoard();
        IsPlaying = true;
        _playerColor = 0;
    }

    public void Restart()
    {
        Turns = new List<Turn> { };
        _board = NewBoard();
        _activeColor = 0;
    }

    private Board NewBoard()
    {
        {
            using (StreamReader r = new StreamReader("test.json"))
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
                Turns = saveGame.Turns;
            }
        }
    }

    public void SaveGame(string fileName)
    {
        var saveGame = new SaveGame(_board.Squares, Turns, _activeColor);
        File.WriteAllText($"savegames/{fileName}.json", JsonSerializer.Serialize(saveGame));

    }

    public void UndoTurn()
    {
        if (Turns.Count == 0)
        {
            throw new Exception("no moves found");
        }
        // revert last move
        var lastTurn = Turns.Last();
        var parsed = ParseMove(lastTurn.Move);
        parsed.Revert();
        _board.MakeMove(parsed.From, parsed.To, parsed.From.Piece!);
        if (lastTurn.Capture is not null)
        {
            // get the square at which the piece was captured
            var square = _board.GetSquareByAddress(lastTurn.Capture.Address);

            // re-populate square with piece
            square.Update(lastTurn.Capture.Piece);
        }
        Turns.RemoveAt(Turns.Count() - 1);
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

    public string PrintBoard(string? msg)
    {

        if (msg is not null)
        {
            msg = "-- " + msg;
        }
        Console.WriteLine(JsonSerializer.Serialize(_board.GetSquareByAddress("e1")));
        return $"\n\n{_board.PrintBoard(_activeColor, _presentation)}\n\n{msg}";
    }

    public string PrintTurns()
    {
        var s = "\nmoves:\n";
        string? capture = null;
        foreach (var turn in Turns)
        {
            capture = turn.Capture is not null ? $"x{turn.Capture.Piece.Type}" : null;
            s += $"{(turn.Piece.Color == 0 ? "w" : "b")}{turn.Piece.Type} {turn.Move}{capture}\n";
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

    // attempt castling of rook at address
    public void Castle(string address)
    {
        if (Castled[_activeColor])
        {
            throw new Exception("Already castled. Each side may only castle once.");
        }
        var rookSquare = _board.GetSquareByAddress(address);
        var rook = rookSquare.Piece;
        if (rook is null)
        {
            throw new Exception("no piece at address");
        }
        if (rook.Type != PieceType.R)
        {
            throw new Exception($"Piece at {address} not of type R");
        }
        if (rook.Color != _activeColor)
        {
            throw new Exception("R not owned by player");
        }
        var kingSquare = _board.GetSquareByAddress(_board.Kings[_activeColor].Address);
        var direction = kingSquare.File - rookSquare.File < 0 ? 1 : -1;
        if (rookSquare.Rank != kingSquare.Rank)
        {
            throw new Exception("Castling only allowed on same rank");
        }
        if (Math.Abs(kingSquare.File - rookSquare.File) < 3)
        {
            throw new Exception("R and K must be at least 2 squares apart");
        }
        _board.MakeMove(kingSquare, _board.Squares[kingSquare.Rank][kingSquare.File + 2 * direction], kingSquare.Piece!);
        _board.MakeMove(rookSquare, _board.Squares[kingSquare.Rank][kingSquare.File + 1 * direction], kingSquare.Piece!);
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
            UndoTurn();
            SwitchTurns();
            throw new CheckError(color, offender.Address, offender.Piece!.Type);
        }
    }

    public void PromotePiece(IChessPiece piece, PieceType promoteTo)
    {
        piece.Promote(promoteTo);
        Promotee = null;
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
        Turns.Add(new Turn(move, piece, capture));

        DetectCheck();

        DetectPromotion(parsed);
    }

    public void Quit()
    {
        IsPlaying = false;
    }
}


public record struct SaveGame(Square[][] Squares, List<Turn> Turns, int ActiveColor) { }



