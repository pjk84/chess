
#nullable enable
namespace Chess.Interfaces;

public interface IChessboard
{

    public Square[][] Squares { get; }

    public int BoardHeight { get; init; }
    public int boardWidth { get; init; }

    public IKing[] Kings { get; }


    public void MakeMove(IChessMove move);
    public void ValidateMove(IChessMove move, int activeColor, (string from, string to)? projection);

    public string Serialize();

    public IThreat? DetectThreat(int color, IChessMove? move);

    public IChessSquare GetSquareByAddress(string address);

    public void PrintBoard(IntPtr window, int activeColor, string cursor, IChessPiece? selectedPiece, bool showOwnArmy = false);

    public List<IChessSquare> Slice(IChessMove move);

    public Square[] GetSquaresByArmy(int color);
    public List<IGrouping<int?, Square>> GroupSquares();


}


