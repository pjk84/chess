
using Chess.Interfaces;
using Mindmagma.Curses;
using System.Linq;
using System.Text.Json;

namespace Chess.Models;


public class Game : IChessGame
{
    private IntPtr _win;
    private IntPtr _textBox;

    public bool ShowOwnArmy { get; set; }
    public int CursorX { get; private set; } = 0;
    public int CursorY { get; private set; } = 0;

    public string? PieceSelectedAt { get; private set; }
    public string? PieceReleasedAt { get; private set; }

    public IChessPiece? SelectedPiece { get; private set; }

    private int _activeColor = 0;
    private int _playerColor;

    public bool _withAi = false;

    private int _presentation = 1;


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
        _textBox = NCurses.NewWindow(5, 56, 30, 1);
        // NCurses.MoveWindow(_textBox, 5, 5);
        NCurses.StartColor();
        NCurses.CBreak();
        NCurses.NoDelay(_win, true);
        NCurses.NoEcho();
        NCurses.SetCursor(0);
        NCurses.Keypad(_win, true);
        NCurses.InitColor(2, 920, 900, 804);
        NCurses.InitColor(3, 216, 160, 104);
        NCurses.InitColor(4, 120, 113, 104);
        NCurses.InitColor(5, 1000, 980, 260);
        NCurses.InitPair(1, 0, 2); // white
        NCurses.InitPair(2, 7, 3); // black
        NCurses.InitPair(3, 7, 4); // edge
        NCurses.InitPair(4, 0, 5); // selection
        NCurses.InitPair(5, 4, 6); // own army

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
        if (square.Piece is null)
        {
            throw new Exception("Square is empty");
        }
        if (square.Piece.Color != _activeColor)
        {
            throw new Exception($"Piece not owned by player {(_activeColor == 0 ? "white" : "black")}");
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
        move = $"{PieceSelectedAt}-{PieceReleasedAt}";
        PieceReleasedAt = null;
        PieceSelectedAt = null;
        SelectedPiece = null;
        // piece not moved
        if (move is not null)
        {
            MakeMove(move);
        }
    }

    public void UndoAction(bool switchTurns = true)
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
        if (switchTurns)
        {

            SwitchTurns();
        }

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

    public void PrintTextBox(string? msg)
    {
        NCurses.ClearWindow(_textBox);
        string? lastMove = null;
        if (Actions.Count() > 0)
        {
            var action = Actions.Last();
            var capture = action.Capture?.Piece.Type;
            lastMove = $"{action.Piece.Type}-{action.Move}{(capture is not null ? $"x{capture}" : null)}";
        }
        if (_board.Check is not null)
        {
            NCurses.MoveWindowAddString(_textBox, 0, 0, $"K of player {(_board.Check.King.Color == 0 ? "White" : "Black")} is threatened");
        }
        var txt = msg ?? $"{(_activeColor == 0 ? "White" : "Black")} is playing{(lastMove is not null ? $", last move was {lastMove}" : null)}";
        NCurses.MoveWindowAddString(_textBox, 1, 0, txt);
        NCurses.WindowRefresh(_textBox);

    }
    public void PrintBoard()
    {

        NCurses.GetMaxYX(_win, out int screenHeight, out int screenWidth);
        // NCurses.ClearWindow(_win);
        if (screenHeight < _board.BoardHeight || screenWidth < _board.boardWidth)
        {
            NCurses.ResizeTerminal(_board.BoardHeight, _board.boardWidth);
        }

        _board.PrintBoard(_win, _activeColor, CursorToAddress(), SelectedPiece, ShowOwnArmy);
        // }
        NCurses.WindowRefresh(_win);
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

    public void ToggleAi()
    {
        _withAi = !_withAi;
        PrintTextBox($"AI {(_withAi ? "on" : "off")}");
    }

    public void SwitchTurns()
    {
        _activeColor = 1 - _activeColor;
        if (!_withAi)
        {
            _playerColor = 1 - _playerColor;
        }
    }

    public void AiMove()
    {
        var aiColor = 1 - _playerColor;
        // resolve check if checked
        if (_board.Check is not null && _board.Check.King.Color == aiColor)
        {
            //get all squares between the threat and the king
            var slice = _board.Slice(new Move(_board.Check.Threat, _board.GetSquareByAddress(_board.Check.King.Address)));


            var army = _board.getSquaresByArmy(aiColor);
            List<IChessMove> options = new List<IChessMove> { };
            foreach (var square in slice)
            {
                foreach (var position in army)
                {
                    try
                    {
                        var move = new Move(position, square);
                        _board.ValidateMove(move, aiColor);
                        options.Add(move);
                    }
                    catch
                    {
                        // move not possible
                    }
                }
            }
            if (options.Count() != 0)
            {
                var m = options[0];
                _board.MakeMove(m.From, m.To, m.From.Piece!);
            }
        }
        DetectCheck();
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
            var check = _board.FindCheck();
            if (check is not null)
            {
                if (check.King.Color == _activeColor)
                {
                    foreach (var i in Enumerable.Range(0, step))
                    {
                        UndoAction(false);
                    }
                    throw new Exception($"Castling under check not allowed. K checked by {check.Threat.Piece!.Type} at {check.Threat.Address} ");
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
        var check = _board.FindCheck();
        if (check is null)
        {
            // neither side is checked. No need to take the analysis any further.
            return;
        }

        // the side that is checked
        if (check.King.Color == _activeColor)
        {
            UndoAction(false);
            throw new CheckError(check);
        }

    }

    public void DetectCheckMate()
    {

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
            catch (TargetError)
            {
                if (parsed.From.Piece?.Type == PieceType.R && parsed.To.Piece?.Type == PieceType.K)
                {
                    // castle
                    Castle(parsed.From.Address);
                    return;
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

            if (_withAi)
            {
                AiMove();
                SwitchTurns();
            }

        }
        catch (MoveError e)
        {
            throw new Exception($"illegal move. {(e.Type is not null ? $"Move is invalid for piece of type {e.Type}." : null)}\n{e.Message}");
        }
        catch (CheckError e)
        {
            DetectCheckMate();
            throw new Exception($"illegal move.\n{(e.Check.King.Color == 0 ? "wK" : "bK")} checked by {e.Check.Threat.Piece!.Type} at {e.Check.Threat.Address} ");
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
        NCurses.EndWin();
    }
}


public record struct SaveGame(Square[][] Squares, List<Action> Actions, int ActiveColor) { }



