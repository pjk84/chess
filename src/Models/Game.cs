
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
            using (StreamReader r = new StreamReader($"games/{fileName}.json"))
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
        File.WriteAllText($"games/{fileName}.json", JsonSerializer.Serialize(saveGame));

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

    public string? MakeMove(string move)
    {
        string? err = null;
        string? msg = null;
        try
        {
            IChessMove? parsed = null;
            try
            {
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
                }
                _board.MakeMove(parsed.From, parsed.To, parsed.From.Piece!);
                Turns.Add(new Turn(move, piece, capture));

                DetectCheck();

                DetectPromotion(parsed);


            }
            catch (MovementError e)
            {
                throw new Exception($"move is invalid for piece of type {e.Type}. {e.Message}");
            }
            catch (CheckError e)
            {
                err = $"{(e.Color == 0 ? "wK" : "bK")} checked by {e.Offender} at {e.Address} ";
            }
            catch (MoveParseError)
            {
                err = "invalid move format. Move must be formatted as <from>-<to>. Example: a2-a3";
            }
            catch (AddressParseError e)
            {
                err = $"invalid address '{e.Address}'";
            }

        }
        catch (Exception e)
        {
            err = $"{e.Message}\n";
        }
        return err is null ? msg : $"illegal move: {err}\n";
    }

    public void Quit()
    {
        IsPlaying = false;
    }
}


public record struct SaveGame(Square[][] Squares, List<Turn> Turns, int ActiveColor) { }



