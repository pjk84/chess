
using Chess.Interfaces;
namespace Chess.Models;

public record Capture : ICapture
{

    public Piece Piece { get; init; }

    public string Address { get; init; }

    public Capture(Piece piece, string address)
    {
        Piece = piece;
        Address = address;
    }
}