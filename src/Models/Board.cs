
using Chess.Interfaces;
using System.Text.Json;
namespace Chess.Models;


public class Board : IChessboard
{
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

    public string PrintBoard(int activeColor, int presentation)
    {
        var s = "";
        var i = 0;
        foreach (var rank in Squares.Reverse())
        {
            if (i == 0)
            {
                s += "    a   b   c   d   e   f   g   h\n";
                s += $"  ┏━━━┯━━━┯━━━┯━━━┯━━━┯━━━┯━━━┯━━━┓ {(activeColor == 1 ? "<- B" : null)} \n";
            }

            foreach (var square in rank)
            {
                var line = "│";
                if (square.File == 0)
                {
                    s += $"{8 - i} ";
                    line = "┃";
                }
                if (presentation == 0)
                {
                    s += $"{(square.Piece is null ? $"{line}{(square.Color == 0 ? "╳╳╳" : "   ")}" : $"{line} {Pieces[$"{square.Piece.Type}{square.Piece.Color}"]} ")}";
                }
                else
                {

                    s += $"{(square.Piece is null ? $"{line}{(square.Color == 0 ? "╳╳╳" : "   ")}" : $"{line}{square.Piece.Type}{(square.Piece.Color == 0 ? "w" : "b")} ")}";
                }
                // s += $"_{square.Color}_";
                if (square.File == 7)
                {
                    s += $"┃ {square.Rank}\n";
                }

            }
            if (i == 7)
            {
                s += $"  ┗━━━┷━━━┷━━━┷━━━┷━━━┷━━━┷━━━┷━━━┛ {(activeColor == 0 ? "<- W" : null)}\n";
                s += "    a   b   c   d   e   f   g   h";
            }
            else
            {
                s += "  ┣───┼───┼───┼───┼───┼───┼───┼───┫\n";
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
            throw new Exception($"piece at {move.From.Address} is not owned by player {(activeColor == 0 ? "white" : "black")}");
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
    private Square[] Slice(IChessMove move)
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

public record Square : IChessSquare
{

    public int Rank { get; init; }
    public int File { get; init; }
    public int Color { get; init; }

    public string Address { get; init; } // string representation of square

    public Piece? Piece { get; private set; } = null;

    public void Update(Piece? piece)
    {
        Piece = piece;
    }

    public Square(int file, int rank, Piece piece)
    {

        Rank = rank;
        File = file;
        Piece = piece;
        Color = (file + rank) % 2 == 0 ? 1 : 0;
        var files = "abcdefgh";
        Address = $"{files[file]}{rank + 1}";
    }
}



public record King
{
    public int Color { get; init; }

    public string Address { get; set; }

    public bool Checked { get; set; }
    public King(int color, bool isChecked, string address)
    {
        Color = color;
        Checked = isChecked;
        Address = address;
    }
}