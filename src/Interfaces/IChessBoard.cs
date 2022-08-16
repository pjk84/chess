using Chess.Models;

#nullable enable
namespace Chess.Interfaces;

public interface IChessboard
{
    public Square[][] Squares { get; }

    public int BoardHeight { get; init; }
    public int boardWidth { get; init; }

    public King[] Kings { get; }

    public void MakeMove(IChessSquare from, IChessSquare to, Piece piece);
    public void ValidateMove(IChessMove move, int activeColor);

    public string Serialize();


    public IChessSquare GetSquareByAddress(string address);

    public void PrintBoard(IntPtr window, int activeColor, string cursor, string? lastMove, string? msg, IChessPiece? selectedPiece);
    public (int color, Square square)? EvaluateCheck();

    public Square[] Slice(IChessMove move);


}



