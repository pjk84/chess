using Chess.Models;
using Chess.Interfaces;

public interface IChessSquare
{
    public int Rank { get; init; }
    public int File { get; init; }

    public string Address { get; init; }

    public int Color { get; init; }

    public Piece? Piece { get; }

    public void Update(Piece? piece);

}
