
using Chess.Interfaces;
using System.Text.Json;
namespace Chess.Models;
using Mindmagma.Curses;


public class Board : IChessboard
{

    private string _letters = "abcdefgh";

    private int _squareSize = 6;
    private int _edgeWidth = 4;

    private int _offsetY = 1;
    public int BoardHeight { get; init; }
    public int boardWidth { get; init; }

    public Square[][] Squares { get; private set; }
    public IKing[] Kings { get; private set; } = { new King(0, "e1"), new King(1, "e8") };

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
        BoardHeight = ((_squareSize / 2) * 8) + _edgeWidth;
        boardWidth = (_squareSize * 8) + _edgeWidth * 2;
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

    public void PrintBoard(IntPtr window, int activeColor, string cursor, IChessPiece? selectedPiece, bool showOwnArmy = false)
    {

        int boardWidth = _squareSize * 8 + (_edgeWidth * 2);
        var boardHeight = ((_squareSize * 8) / 2) + _edgeWidth;
        var emptySquareH = string.Concat(Enumerable.Repeat(" ", _squareSize));

        var i = 0;

        string boarderChunkHorizontal = string.Concat(Enumerable.Repeat(" ", _edgeWidth));

        // left corner chunk
        var boarderFiles = boarderChunkHorizontal;
        var boarderHClear = string.Concat(Enumerable.Repeat(" ", boardWidth - 3));
        string boarderV = string.Concat(Enumerable.Repeat(" ", _edgeWidth));
        foreach (var rank in Squares.Reverse())
        {
            // print left and right boarder
            if (i == 0)
            {
                NCurses.AttributeOn(NCurses.ColorPair(3));
                NCurses.MoveWindowAddString(window, 1 + _offsetY, 0, $"{boarderHClear}{(activeColor == 1 ? "<< " : "   ")}");
            }
            if (i == 7)
            {
                NCurses.TouchWindow(window);
                NCurses.AttributeOn(NCurses.ColorPair(3));
                NCurses.MoveWindowAddString(window, (boardHeight - _edgeWidth / 2) + _offsetY, 0, $"{boarderHClear}{(activeColor == 0 ? "<< " : "   ")}");
            }

            var file = 0;
            foreach (var square in rank)
            {
                var focused = square.Address == cursor;
                var squareColor = focused ? 4 : square.Color + 1;

                if (i == 0)
                {
                    boarderFiles += $"  {_letters[file]}   ";
                    if (file == 7)
                    {
                        // add right corner chunk to finish the row
                        boarderFiles += boarderChunkHorizontal;
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        NCurses.MoveWindowAddString(window, 0 + _offsetY, 0, boarderFiles);
                    }
                }
                if (i == 7)
                {
                    if (file == 7)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        NCurses.MoveWindowAddString(window, (boardHeight - _edgeWidth / 2) + 1 + _offsetY, 0, boarderFiles);
                    }
                }
                var boarderNumeralOffsetY = (i * _squareSize / 2 + (_edgeWidth / 2 + 1));
                var SquareOffsetX = file * _squareSize + _edgeWidth;
                var RightBoarderOffsetX = (8 * _squareSize) + _edgeWidth;
                var piece = focused ? selectedPiece ?? square.Piece : square.Piece;
                var pieceNotation = "  ";
                if (piece is not null)
                {
                    if (showOwnArmy && piece.Color == activeColor && !focused)
                    {
                        squareColor = 5;
                    }
                    pieceNotation = $"{(piece.Color == 0 ? "_" : " ")}{piece.Type}";
                    if (!focused && piece.Id == selectedPiece?.Id)
                    {
                        pieceNotation = "  ";
                    }
                }

                foreach (var h in Enumerable.Range(1, 3))
                {

                    // draw left boarder with numeral
                    if (file == 0)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        if (h == 2)
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY + _offsetY, 0, $" {8 - i}  ");
                        }
                        else
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY - (2 - h) + _offsetY, 0, boarderV);
                        }
                    }

                    // draw right boarder with numeral
                    if (file == 7)
                    {
                        NCurses.AttributeOn(NCurses.ColorPair(3));
                        if (h == 2)
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY + _offsetY, RightBoarderOffsetX, $"  {8 - i} ");
                        }
                        else
                        {
                            NCurses.MoveWindowAddString(window, boarderNumeralOffsetY - (2 - h) + _offsetY, RightBoarderOffsetX, boarderV);

                        }

                    }
                    NCurses.AttributeOn(NCurses.ColorPair(squareColor));

                    if (h == 2)
                    {
                        NCurses.MoveWindowAddString(window, boarderNumeralOffsetY + _offsetY, SquareOffsetX, $"  {pieceNotation}  ");
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
        NCurses.AttributeOff(NCurses.ColorPair(1));
        NCurses.AttributeOff(NCurses.ColorPair(2));


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


    public Square[] GetSquaresByArmy(int color)
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

    public List<IGrouping<int?, Square>> GroupSquares()
    {
        List<Square> squares = new List<Square>();
        foreach (var rank in Squares)
        {
            foreach (var square in rank)
            {

                squares.Add(square);
            }
        }
        return squares.GroupBy(s => s.Piece?.Color).ToList();
    }


    // detect threat by color if king is moved to new address
    public IThreat? DetectThreat(int color, IChessMove? move)
    {
        {
            (string, string)? projection = null;
            if (move is not null)
            {
                projection = (move.From.Address, move.To.Address);
            }
            var king = Kings[color];
            var address = king.Address;
            if (move?.From.Piece is not null)
            {
                // check if active player king is threatened after moving its position
                if (move.From.Piece.Type == PieceType.K && move.From.Piece.Color == color)
                {
                    address = move.To.Address;
                }
            }
            var kingsSquare = GetSquareByAddress(address);
            var _i = color == 0 ? 1 : 0;
            var squares = GetSquaresByArmy(_i);
            foreach (var square in squares)
            {
                try
                {
                    ValidateMove(new Move(square, kingsSquare), _i, projection);
                    return new Threat(square, king);
                }
                catch (Exception)
                {
                    // Console.WriteLine($"{square.Piece?.Type} at {square.Address}: {e.Message}");
                }
            }
            // no threat
            return null;
        }
    }

    // checks move against game rules at the board and piece level.
    // optional projection to validate against hypothetical move
    public void ValidateMove(IChessMove move, int activeColor, (string from, string to)? projection)
    {

        if (move.From.Address == move.To.Address)
        {
            throw new MoveError(null, "start and destination may not be the same square");
        }
        if (move.From.Piece is null)
        {
            throw new Exception($"square is empty");
        }

        if (move.From.Piece.Color != activeColor)
        {
            throw new MoveError(null, $"{move.From.Piece.Type} at {move.From.Address} is not owned by player {(activeColor == 0 ? "white" : "black")}");
        }

        move.From.Piece.ValidateMove(move, move.To.Piece);


        if (move.To.Piece?.Color == activeColor)
        {

            throw new TargetError();
        }

        //bounds
        MoveIsWithinBounds(move.To);


        CheckCollision(move, projection);

    }


    private void CheckCollision(IChessMove move, (string from, string to)? projection)
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
                if (projection?.from == square.Address)
                {
                    // this square is projected to be empty when the validated move occurs.
                    continue;
                }
                if (square.Piece is not null || projection?.to == square.Address)
                {

                    throw new CollisionError($"blocked by {square.Piece!.Type} at {square.Address}", move.From.Piece!, square);
                }
            }
        }
    }

    // return single array of squares between from and to by move direction
    //
    public List<IChessSquare> Slice(IChessMove move)
    {

        List<IChessSquare> slice = new List<IChessSquare>() { };
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

                    if (s.Address == move.From.Address || s.Address == move.To.Address)
                    {
                        // eliminate source and destination and squares that are not on the diagonal
                        continue;
                    }
                    if (Math.Abs(s.Rank - move.From.Rank) != Math.Abs(s.File - move.From.File))
                    {
                        // eliminate not diagonal
                        continue;
                    }
                    slice.Add(s);

                }
            }
            return slice;
        }
        if (move.From.File != move.To.File)
        {
            // horizontal move. no transposition needed
            range = new[] { move.From.File, move.To.File };
            var rank = (IChessSquare[])Squares[move.From.Rank];
            slice = rank.ToList();

        }
        else
        {
            // vertical move
            range = new[] { move.From.Rank, move.To.Rank };
            foreach (var i in Enumerable.Range(0, 8))
            {
                var s = Squares[i][move.From.File];
                slice.Add(s);
            }
        }

        return slice.Take((range.Min() + 1)..range.Max()).ToList();
    }



    public void MakeMove(IChessMove move)
    {
        var piece = move.From.Piece!;
        move.To.Update(piece);

        move.From.Update(null);

        if (piece.Type == PieceType.K)
        {
            var king = Kings.First(k => k.Color == piece.Color);
            king.Address = move.To.Address;
        }

    }
}

