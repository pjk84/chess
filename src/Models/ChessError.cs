
using Chess.Interfaces;


class CollisionError : Exception, ICollisonError
{
    public IChessPiece Mover { get; init; }
    public IChessSquare Square { get; init; }
    public CollisionError(string message, IChessPiece mover, IChessSquare square) : base(message)
    {
        Mover = mover;
        Square = square;
    }
}

class MoveParseError : Exception { }

class CheckError : Exception
{
    public int Color { get; init; }
    public string Address { get; init; }

    public PieceType Offender { get; init; }
    public CheckError(int color, string address, PieceType offender)
    {
        Color = color;
        Address = address;
        Offender = offender;
    }
}

class MovementError : Exception
{
    public PieceType Type { get; init; }

    public MovementError(PieceType type, string? message) : base(message)
    {
        Type = type;
    }
}


class AddressParseError : Exception
{
    public string Address { get; init; }
    public AddressParseError(string address)
    {
        Address = address;
    }
}
