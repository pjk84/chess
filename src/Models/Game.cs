
using Chess.Interfaces;
using Mindmagma.Curses;
using System.Text.Json;

namespace Chess.Models;


public class Game : IChessGame
{
    private IntPtr _win;

    public int CursorX { get; private set; } = 0;
    public int CursorY { get; private set; } = 0;

    public string? PieceSelectedAt { get; private set; }
    public string? PieceReleasedAt { get; private set; }

    public IChessPiece? SelectedPiece { get; private set; }

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
        NCurses.NoDelay(_win, true);
        NCurses.NoEcho();
        NCurses.Keypad(_win, true);
        NCurses.WindowRefresh(_win);
        NCurses.StartColor();
        NCurses.InitColor(2, 920, 900, 804);
        NCurses.InitColor(3, 216, 160, 104);
        NCurses.InitColor(4, 120, 113, 104);
        NCurses.InitColor(5, 1000, 980, 260);
        NCurses.InitPair(1, 0, 2); // white
        NCurses.InitPair(2, 7, 3); // black
        NCurses.InitPair(3, 7, 4); // edge
        NCurses.InitPair(4, 7, 5); // edge
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

    public void SetCursor(System.ConsoleKey arrowKey)
    {

        if (arrowKey == ConsoleKey.RightArrow)
        {
            CursorX += 1;
            if (CursorX > 7)
            {
                CursorX = 0;
            }
        }
        if (arrowKey == ConsoleKey.LeftArrow)
        {
            CursorX -= 1;
            if (CursorX < 0)
            {
                CursorX = 7;
            }
        }
        if (arrowKey == ConsoleKey.UpArrow)
        {
            CursorY += 1;
            if (CursorY > 7)
            {
                CursorY = 0;
            }
        }
        if (arrowKey == ConsoleKey.DownArrow)
        {
            CursorY -= 1;
            if (CursorY < 0)
            {
                CursorY = 7;
            }
        }
        return;
    }

    private string CursorToAddress()
    {
        var letter = "abcdefgh";
        return $"{letter[CursorX]}{CursorY + 1}";
    }


    public void SelectPiece()
    {
        var address = CursorToAddress();
        var square = _board.GetSquareByAddress(address);
        if (square.Piece?.Color != _activeColor)
        {
            throw new Exception("Piece not owned by active player");
        }
        PieceSelectedAt = CursorToAddress();
        SelectedPiece = square.Piece;
    }

    public void ReturnPiece()
    {
        SelectedPiece = null;
        PieceSelectedAt = null;
    }

    public void MovePiece()
    {
        string? move = null;
        PieceReleasedAt = CursorToAddress();
        if (PieceReleasedAt != PieceSelectedAt)
        {
            move = $"{PieceSelectedAt}-{PieceReleasedAt}";
        }
        PieceReleasedAt = null;
        PieceSelectedAt = null;
        SelectedPiece = null;
        // piece not moved
        if (move is not null)
        {
            MakeMove(move);
        }
    }

    public void UndoAction()
    {
        if (Actions.Count == 0)
        {
            throw new Exception("Cannot undo. No moves found..");
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
        NCurses.GetMaxYX(_win, out int screenHeight, out int screenWidth);
        NCurses.ClearWindow(_win);
        if (screenHeight < _board.BoardHeight)
        {
            NCurses.MoveWindowAddString(_win, 1, 1, "not enough room to render. Terminal height is insufficient.\n Please redize terminal. Press any key to attempt a re-render.");
        }
        else if (screenHeight < _board.BoardHeight)
        {
            NCurses.MoveWindowAddString(_win, 1, 1, "not enough room to render. Terminal width is insufficient.\n Please redize terminal. Press any key to attempt a re-render.");
        }
        else
        {
            string? lastMove = null;
            if (Actions.Count() > 0)
            {
                var action = Actions.Last();
                var capture = action.Capture?.Piece.Type;
                lastMove = $"{action.Move}{(capture is not null ? $"x{capture}" : null)}";
            }
            _board.PrintBoard(_win, _activeColor, CursorToAddress(), lastMove, msg, SelectedPiece);
        }
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
                throw new Exception(e.Message);
            }
            _board.MakeMove(parsed.From, parsed.To, parsed.From.Piece!);
            Actions.Add(new Action(piece, move, capture, null, false));

            DetectCheck();

            DetectPromotion(parsed);

        }
        catch (MovementError e)
        {
            throw new Exception($"move is invalid for piece of type {e.Type}. {e.Message}");
        }
        catch (CheckError e)
        {
            throw new Exception($"{(e.Color == 0 ? "Kw" : "Kb")} checked by {e.Offender} at {e.Address} ");
        }
        catch (MoveParseError)
        {
            throw new Exception("invalid move format. Move must be formatted as <from>-<to>. Example: a2-a3");
        }
        catch (AddressParseError e)
        {
            throw new Exception($"invalid address '{e.Address}'");
        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }
    }

    public void Quit()
    {
        IsPlaying = false;
    }
}


public record struct SaveGame(Square[][] Squares, List<Action> Actions, int ActiveColor) { }



