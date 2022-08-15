using Chess.Models;
namespace Chess.Interfaces;

public interface IChessGame
{
    public string? Cursor { get; }

    public string? PieceSelectedAt { get; }

    public string? PieceReleasedAt { get; }

    public bool IsPlaying { get; }

    public string? Checked { get; }
    public Piece? Promotee { get; set; }

    public bool[] Castled { get; }

    public List<Action> Actions { get; }

    public void PrintBoard(string? msg);

    public string PrintTurns();

    public void SetCursor();

    public void SelectPiece();

    public void ReleasePiece();

    public void Castle(string address);

    public void SaveGame(string fileName);

    public void LoadGame(string fileName);

    public void MakeMove(string move);

    public void UndoAction();

    public void Setup();

    public void SwitchTurns();

    public void Quit();
}


public interface ISaveGame
{
    public Board Board { get; init; }

    public List<string> Moves { get; init; }

    public Color ActiveColor { get; init; }

}