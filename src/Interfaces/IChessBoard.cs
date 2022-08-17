using Chess.Models;

#nullable enable
namespace Chess.Interfaces;

public interface IChessboard
{

    public Square[][] Squares { get; }

    public int BoardHeight { get; init; }
    public int boardWidth { get; init; }

    public IKing[] Kings { get; }


    public void MakeMove(IChessMove move);
    public void ValidateMove(IChessMove move, int activeColor);

    public string Serialize();


    public IChessSquare GetSquareByAddress(string address);

    public void PrintBoard(IntPtr window, int activeColor, string cursor, IChessPiece? selectedPiece, bool showOwnArmy = false);

    public List<IChessSquare> Slice(IChessMove move);

    public Square[] getSquaresByArmy(int color);
    public List<IGrouping<int?, Square>> groupSquares();


}


