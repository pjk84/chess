using Chess.Models;
namespace Chess.Interfaces;

public interface IChessGame
{

    public bool IsPlaying { get; }

    public string? Checked { get; }
    public IChessPiece? Promotee { get; }

    public List<Turn> Turns { get; }

    public string PrintBoard(string? msg);

    public string PrintTurns();

    public void SaveGame(string fileName);
    public void LoadGame(string fileName);

    public string? MakeMove(string move);

    public void PromotePiece(IChessPiece piece, PieceType type);

    public void UndoTurn();

    public void Restart();

    public void SwitchTurns();

    public void Quit();
}


public interface ISaveGame
{
    public Board Board { get; init; }

    public List<string> Moves { get; init; }

    public Color ActiveColor { get; init; }

}