using Chess.Models;
namespace Chess.Interfaces;

public interface ICapture
{
    public Piece Piece { get; }

    public string Address { get; }
}
