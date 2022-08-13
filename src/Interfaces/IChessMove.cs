namespace Chess.Interfaces;

public interface IChessMove
{
    public IChessSquare From { get; set; }
    public IChessSquare To { get; set; }

    public int Length { get; init; }
    public int Width { get; init; }

    public MoveType Type { get; init; }

    public void Revert();

}

public enum MoveType
{
    Diagonal,
    Straight,
    Wild
}