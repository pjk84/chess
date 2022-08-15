using Chess.Models;

#nullable enable
namespace Chess.Interfaces;

public interface IChessboard
{
    public Square[][] Squares { get; }

    public King[] Kings { get; }

    public void MakeMove(IChessSquare from, IChessSquare to, Piece piece);
    public void ValidateMove(IChessMove move, int activeColor);

    public string Serialize();


    public IChessSquare GetSquareByAddress(string address);

    public string PrintBoard(IntPtr window, string cursor, string? msg);
    public (int color, Square square)? EvaluateCheck();

    public Square[] Slice(IChessMove move);


}



