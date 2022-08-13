using Chess.Interfaces;

namespace Chess.Models;

public record Move : IChessMove
{

    public int Id { get; init; }

    public int Length { get; init; }
    public int Width { get; init; }

    public MoveType Type { get; init; }

    public IChessSquare From { get; set; }
    public IChessSquare To { get; set; }


    public Move(IChessSquare from, IChessSquare to)
    {
        From = from;
        To = to;
        Length = Math.Abs(to.Rank - from.Rank);
        Width = Math.Abs(to.File - from.File);
        Type = (Length != 0 && Width != 0) ? Length == Width ? MoveType.Diagonal : MoveType.Wild : MoveType.Straight;
    }

    public void Revert()
    {
        var t = From;
        From = To;
        To = t;
    }
}
