
using Chess.Interfaces;
using System.Text.Json;
namespace Chess.Models;
using Mindmagma.Curses;


public class Board : IChessboard
{

    private string _letters = "abcdefgh";
    private int _offsetX = 10;
    private int _offsetY = 2;
    private int _squareSize = 6;
    private int _edgeWidth = 4;

    public Square[][] Squares { get; private set; }
    public King[] Kings { get; private set; } = { new King(0, false, "e1"), new King(1, false, "e8") };
    private Dictionary<string, string> Pieces;
    public Board(string squares)
    {

        Pieces = new Dictionary<string, string>{
            {"P1",  "♙"}, // (P)awn (w)hite .. etc
            {"P0",  "♟"},
            {"K1",  "♔"},
            {"K0",  "♚"},
            {"Q1",  "♕"},
            {"Q0",  "♛"},
            {"N1",  "♘"},
            {"N0",  "♞"},
            {"B1",  "♗"},
            {"B0",  "♝"},
            {"R1",  "♖"},
            {"R0",  "♜"},
        };

        Squares = Deserialize(squares);
    }


    private Square[][] Deserialize(string squares)
    {
        var deserialized = JsonSerializer.Deserialize<Square[][]>(squares, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        if (deserialized is null)
        {
            throw new Exception("could not deserialize board string representation");
        }
        return deserialized;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(Squares);
    }


    public IChessSquare GetSquareByAddress(string address)
    {
        address = address.ToLower();
        var files = "abcdefgh";
        if (address.Length != 2)
        {
            throw new AddressParseError(address);
        }
        var file = files.IndexOf(address[0]);
        if (file == -1)
        {
            throw new AddressParseError(address);
        }
        // var rank = Convert.ToInt32(address[1].ToString());
        int.TryParse(address[1].ToString(), out int rank);
        if (!Enumerable.Range(1, 8).Contains(rank))
        {
            throw new AddressParseError(address);
        }
        return Squares[rank - 1][file];
    }

    public string PrintBoard(IntPtr window, string? cursor, string? msg)
    {
        NCurses.MoveWindowAddString(window, 0, _offsetX, $"{cursor}");
        if (msg is not null)
        {
            NCurses.MoveWindowAddString(window, 0, _offsetX, msg);
        }
        int boardWidth = _squareSize * 8 + (_edgeWidth * 2);
        var boardHeight = ((_squareSize * 8) / 2) + _edgeWidth;
        var emptySquareH = string.Concat(Enumerable.Repeat(" ", _squareSize));

        var s = "";
        var i = 0;

        string boarderChunkHorizontal = string.Concat(Enumerable.Repeat(" ", _edgeWidth));

        // left corner chunk
        var boarderFiles = boarderChunkHorizontal;
        var boarderHClear = string.Concat(Enumerable.Repeat(" ", boardWidth));
        string boarderV = string.Concat(Enumerable.Repeat(" ", _edgeWidth));
        foreach (var rank in Squares.Reverse())
        {
            // print left and right boarder
            if (i == 0)
            {
                NCurses.AttributeOn(NCurses.ColorPair(3));
                NCurses.MoveWindowAddString(window, 1 + _offsetY, 0 + _offsetX, boarderHClear);
            }
            if (i == 7)
            {

                NCurses.AttributeOn(NCurses.ColorPair(3));
                NCurses.MoveWindowAddString(window, (boardHeight - _edgeWidth / 2) + _offsetY, 0 + _offsetX, boarderHClear);
            }

            var file = 0;
            foreach (var square in rank)
            {
                var squareColor = square.Address == cursor ? 4 : square.Color + 1;
                if (i == 0)
                {
                    boarderFiles += $"  {_letters[file]}   ";
                    if (file == 7)
                    {
                        // add right corner chunk to finish the row
                        boarderFiles += boarderChunkHorizontal;
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        NCurses.MoveWindowAddString(window, 0 + _offsetY, 0 + _offsetX, boarderFiles);
                    }
                }
                if (i == 7)
                {
                    if (file == 7)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        NCurses.MoveWindowAddString(window, (boardHeight + 1 - _edgeWidth / 2) + _offsetY, 0 + _offsetX, boarderFiles);
                    }
                }
                var boarderNumeralOffsetY = (i * _squareSize / 2 + (_edgeWidth / 2 + 1)) + _offsetY;
                var SquareOffsetX = file * _squareSize + _edgeWidth + _offsetX;
                var RightBoarderOffsetX = (8 * _squareSize) + _offsetX + _edgeWidth;
                var piece = square.Piece is not null ? $"{(square.Piece.Color == 0 ? "w" : "b")}{square.Piece.Type}" : "  ";
                foreach (var h in Enumerable.Range(1, 3))
                {

                    // draw left boarder with numeral
                    if (file == 0)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        if (h == 2)
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY, _offsetX, $" {8 - i}  ");
                        }
                        else
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY - (2 - h), _offsetX, boarderV);
                        }
                    }

                    // draw right boarder with numeral
                    if (file == 7)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        if (h == 2)
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY, RightBoarderOffsetX, $"  {8 - i} ");
                        }
                        else
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY - (2 - h), RightBoarderOffsetX, boarderV);

                        }
                    }
                    NCurses.AttributeOn(NCurses.ColorPair(squareColor));
                    if (h == 2)
                    {
                        NCurses.MoveWindowAddString(window, boarderNumeralOffsetY, SquareOffsetX, $"  {piece}  ");
                    }
                    else
                    {
                        NCurses.MoveWindowAddString(window, (i * _squareSize / 2 + h + 1) + _offsetY, SquareOffsetX, emptySquareH);
                    }
                }
                file++;
            }


            i++;
        }
        return s;
    }

    private void MoveIsWithinBounds(IChessSquare target)
    {
        var isInBounds = true;
        if (!Enumerable.Range(0, 8).Contains(target.Rank))
        {
            isInBounds = false;
        }
        if (!Enumerable.Range(0, 8).Contains(target.File))
        {
            isInBounds = false;
        }
        if (!isInBounds)
        {
            throw new Exception("out of bounds");
        }
    }

    // if either side is checked, returns a tuple with that sides' color 
    // and the offending square.
    public (int color, Square square)? EvaluateCheck()
    {

        int n = 0;
        foreach (var i in Enumerable.Range(0, 2))
        {
            var kingsSquare = GetSquareByAddress(Kings[i].Address);
            var _i = i == 0 ? 1 : 0;
            var squares = getSquaresByArmy(_i);
            foreach (var square in squares)
            {
                try
                {
                    ValidateMove(new Move(square, kingsSquare), _i);
                    return new(n, square);
                }
                catch (Exception)
                {
                    // Console.WriteLine($"{square.Piece?.Type} at {square.Address}: {e.Message}");
                }
            }
            n++;
        }
        return null;
    }

    private Square[] getSquaresByArmy(int color)
    {
        List<Square> squares = new List<Square>();
        foreach (var rank in Squares)
        {
            foreach (var square in rank)
            {
                if (square.Piece?.Color == color)
                {
                    squares.Add(square);
                }
            }
        }
        return squares.ToArray();
    }

    // checks move against game rules at the board and piece level.
    // Does not account for check. Check is evaluated separately in the game scope.
    public void ValidateMove(IChessMove move, int activeColor)
    {

        if (move.From.Piece is null)
        {
            throw new Exception($"square is empty");
        }

        if (move.From.Piece.Color != activeColor)
        {
            throw new Exception($"{move.From.Piece.Type} at {move.From.Address} is not owned by player {(activeColor == 0 ? "white" : "black")}");
        }

        if (move.To.Piece?.Color == activeColor)
        {
            throw new Exception("own piece found at target square");
        }

        //bounds
        MoveIsWithinBounds(move.To);

        move.From.Piece.ValidateMove(move, move.To.Piece);

        CheckCollision(move);


    }


    private void CheckCollision(IChessMove move)
    {
        if (move.Type == MoveType.Wild)
        {
            // no collision 
            return;
        }
        if (move.Width < 2 && move.Length < 2)
        {
            // move to adjecent square. no collision
            return;
        }
        var squares = Slice(move);
        {
            foreach (var square in squares)
            {
                if (square.Piece is not null)
                {
                    throw new CollisionError($"blocked by {square.Piece.Type} at {square.Address}", move.From.Piece!, square);
                }
            }
        }
    }

    // return single array of squares by move direction
    public Square[] Slice(IChessMove move)
    {
        var slice = Enumerable.Empty<Square>();
        int[] range = { };
        if (move.Type == MoveType.Diagonal)
        {
            // create subset of squares by move diagonal
            var minFile = Math.Min(move.From.File, move.To.File);
            var minRank = Math.Min(move.From.Rank, move.To.Rank);
            foreach (var i in Enumerable.Range(minRank, move.Width + 1))
            {

                foreach (var k in Enumerable.Range(minFile, move.Width + 1))
                {

                    var s = Squares[i][k];

                    // eliminate source and destination and squares that are not on the diagonal
                    if (s.Address == move.From.Address || s.Address == move.To.Address)
                    {
                        continue;
                    }
                    if (Math.Abs(s.Rank - move.From.Rank) != Math.Abs(s.File - move.From.File))
                    {
                        continue;
                    }
                    slice = slice.Append(s);

                }
            }
            return slice.ToArray();
        }
        if (move.From.File != move.To.File)
        {
            // horizontal move. no transposition needed
            range = new[] { move.From.File, move.To.File };
            slice = Squares[move.From.Rank];
        }
        else
        {
            // vertical move
            range = new[] { move.From.Rank, move.To.Rank };
            foreach (var i in Enumerable.Range(0, 8))
            {
                var s = Squares[i][move.From.File];
                slice = slice.Append(s);
            }
        }
        return slice.Take((range.Min() + 1)..range.Max()).ToArray();
    }



    public void MakeMove(IChessSquare from, IChessSquare to, Piece piece)
    {
        to.Update(piece);

        from.Update(null);


        if (piece.Type == PieceType.K)
        {
            var king = Kings.First(k => k.Color == piece.Color);
            king.Address = to.Address;
        }

    }
}


