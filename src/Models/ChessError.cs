
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
    public IThreat Threat { get; init; }
    public CheckError(IThreat threat)
    {
        Threat = threat;
    }
}

class MoveError : Exception
{
    public PieceType? Type { get; init; }

    public MoveError(PieceType? type, string? message) : base(message)
    {
        Type = type;
    }
}

class TargetError : Exception { }



class AddressParseError : Exception
{
    public string Address { get; init; }
    public AddressParseError(string address)
    {
        Address = address;
    }
}
