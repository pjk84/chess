namespace Chess.Interfaces;

#nullable enable 

public interface IChessPiece
{
    public int Id { get; init; }
    // public ChessPieceType Type { get; init; }
    public int Color { get; init; }

    public PieceType Type { get; }


    public void ValidateMove(IChessMove move, IChessPiece? pieceAtTarget);

    public void Promote(PieceType type);
}


public enum PieceType
{
    P, // Pawn
    K, // King
    Q, //Queen
    N, // Knight
    R, // Rook
    B, //Bishop

}

public enum Color
{
    W,
    B
}
