using Chess.Models;
namespace Chess.Interfaces;

public interface IChessGame
{
    public int CursorX { get; }
    public int CursorY { get; }

    public int? CheckMate { get; }

    public string? PieceSelectedAt { get; }

    public string? PieceReleasedAt { get; }

    public IChessPiece? SelectedPiece { get; }

    public bool Ai { get; }

    public bool IsPlaying { get; }


    public Piece? Promotee { get; set; }


    public bool ShowOwnArmy { get; set; }

    public List<Action> Actions { get; }

    public void PrintBoard();
    public void PrintText(string? msg);

    public string PrintTurns();

    public void SetCursor(System.ConsoleKey arrowKey);

    public void SelectPiece();

    public void MovePiece();

    public void ToggleAi();
    public void ReturnPiece();

    public void Castle(string address);

    public void SaveGame(string fileName);

    public void LoadGame(string fileName);

    public void MakeMove(string move);
    public void AiMove();

    public void UndoAction(bool switchTurns = true);

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



public interface IThreat
{
    public IChessSquare From { get; init; }
    public IKing King { get; init; }
}