
using Chess.Interfaces;

namespace Chess.Models;


#nullable enable

public class Piece : IChessPiece
{
    public int Id { get; init; }
    public PieceType Type { get; private set; }

    public int Color { get; init; }

    private IChessMove[] Moves { get; } = { };

    public Piece(int id, int color, PieceType type)
    {
        Id = id;
        Color = color;
        Type = type;
    }

    public void Promote(PieceType type)
    {
        Type = type;
    }

    // pattern and behavior validation
    // Does not account for board level bounds or collisions
    public void ValidateMove(IChessMove move, IChessPiece? pieceAtTarget)
    {
        string? err = null;
        if (move.Type == MoveType.Wild && Type != PieceType.N)
        {
            // wild move. if knight, evaluate on knight case
            err = "invalid pattern";
        }
        switch (Type)
        {
            case PieceType.P:

                if (move.Length > 2 || move.Width > 2)
                {
                    err = $"move size exceeds max allowed size of 2";
                }
                if (move.Type == MoveType.Diagonal)
                {
                    if (move.Width > 1)
                    {
                        err = "move size exceeds max allowed size of 1";
                    }
                    if (pieceAtTarget is null)
                    {

                        err = "Pawn may not move diagonally unless attacking";
                    }
                }
                if (move.Type == MoveType.Straight)
                {
                    if (move.Length == 2)
                    {
                        if (Moves.Length > 0)
                        {
                            err = "Pawn may only advance two squares on the first move";
                        }
                    }
                    if (move.Width > 0)
                    {

                        err = "Pawn may only advance vertically";
                    }
                    if (pieceAtTarget is not null)
                    {
                        err = "Pawn may only attack diagonally or en passant";
                    }
                }
                if (Color == 1)
                {
                    if (move.To.Rank > move.From.Rank)
                    {
                        err = "Pawn may not move backwards";
                    }
                }
                if (Color == 0)
                {
                    if (move.To.Rank < move.From.Rank)
                    {
                        err = "Pawn may not move backwards";
                    }
                }

                // implement en passant edge case
                break;
            case PieceType.R:
                if (move.Type != MoveType.Straight)
                {
                    err = "diagonal move not allowed";
                }
                break;
            case PieceType.B:
                if (move.Type != MoveType.Diagonal)
                {
                    err = "only diagonal move allowed";
                }
                break;
            case PieceType.Q:
                // only wild move not allowed.
                break;
            case PieceType.K:
                // only wild move not allowed.
                if (move.Length > 1 || move.Width > 1)
                {
                    err = $"move size exceeds max of 1";
                }
                // implement castling
                break;
            case PieceType.N:
                // only wild move not allowed.
                if (move.Type != MoveType.Wild)
                {
                    err = $"straight or diagonal move not allowed";
                }

                if (!((move.Length == 1 || move.Width == 1) && move.Length + move.Width == 3))
                {
                    err = $"illegal movement pattern";
                }
                // implement castling
                break;
            default:
                break;
        }
        if (err is not null)
        {
            throw new MovementError(Type, err);
        }
    }

}


public record King
{
    public int Color { get; init; }

    public string Address { get; set; }

    public bool Checked { get; set; }
    public King(int color, bool isChecked, string address)
    {
        Color = color;
        Checked = isChecked;
        Address = address;
    }
}


