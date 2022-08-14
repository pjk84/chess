using Chess.Models;
namespace Chess.Interfaces;

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
