

namespace Chess.Interfaces;

interface ICollisonError
{
    IChessPiece Mover { get; init; }

    IChessSquare Square { get; init; }
}


interface IParseError
{
    ParseType Type { get; init; }
}

public enum ParseType
{
    Move,
    Address
}