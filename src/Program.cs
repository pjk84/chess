using Chess.Models;

using Chess.Interfaces;
class Demo
{

    static void Main()
    {
        string? inp = null;
        string? msg = null;
        IChessGame game = new Game();
        void print(string? msg)
        {
            Console.WriteLine(game.PrintBoard(msg));
            msg = null;
        }
        Console.WriteLine("commands:");
        Console.WriteLine("moves  --> show sequence of moves");
        Console.WriteLine("save  --> save game");
        Console.WriteLine("load  --> load game");
        Console.WriteLine("reset --> reset game");
        Console.WriteLine("exit  --> quit game");
        Console.WriteLine(game.PrintBoard(null));
        while (game.IsPlaying)
        {
            Console.Write("make a move: ");
            inp = Console.ReadLine()?.ToLower();
            if (inp is null)
            {
                continue;
            }
            if (inp == "load")
            {
                string[] files = Directory.GetFiles(@"games", "*.json");
                Console.WriteLine("\nfound games: \n");
                foreach (var file in files)
                {
                    var f = file.Replace("games/", "").Replace(".json", "");
                    Console.WriteLine($"- {f}");
                }
                Console.Write($"\nfound {files.Length} savegames. Which game do you want to continue? ");
                string? fileName = Console.ReadLine()?.ToLower();
                msg = $"succesfully loadded game {fileName}";
                if (fileName is null)
                {
                    throw new Exception("-- invalid file name");
                }
                try
                {
                    game.LoadGame(fileName);
                }
                catch (Exception e)
                {
                    msg = $"could not load game: {e.Message}";
                }
                print(msg);
                continue;
            }
            if (inp == "moves")
            {
                if (game.Turns.Count == 0)
                {
                    Console.WriteLine("no moves found..");
                    continue;
                }
                var moves = game.PrintTurns();
                Console.WriteLine(moves);
                continue;
            }
            if (inp == "save")
            {
                Console.Write("enter filename. type 'skip' to continue without saving: ");
                string? fileName = Console.ReadLine();

                if (fileName is null)
                {
                    throw new Exception("-- invalid file name");
                }
                if (fileName.ToLower() == "skip")
                {
                    continue;
                }
                game.SaveGame(fileName);
                Console.WriteLine($"-- game saved as '{fileName}'. To load game use command 'load test'.");
                continue;
            }
            if (inp == "quit" || inp == "exit")
            {
                game.Quit();
                return;
            }
            if (inp == "restart" || inp == "reset")
            {
                game.Restart();
                print("game reset");
                continue;
            }
            if (inp == "undo")
            {
                if (game.Turns.Count() == 0)
                {
                    Console.WriteLine("no turns found.. ");
                    continue;
                }
                bool done = false;
                while (!done)
                {
                    msg = $" found {game.Turns.Count()} move(s). Press delete/backspace to undo move. Press enter when done.";
                    msg += game.PrintTurns();
                    print(msg);
                    var e = Console.ReadKey();
                    if (e.Key == ConsoleKey.Enter)
                    {
                        done = true;
                        continue;
                    }
                    if (e.Key == ConsoleKey.Backspace)
                    {
                        game.UndoTurn();
                        if (game.Turns.Count() == 0)
                        {
                            done = true;
                        }
                    }

                }
                print(null);
                continue;
            }
            msg = game.MakeMove(inp);
            if (game.Checked is not null)
            {
                msg += $"player {game.Checked} is checked";
            }
            if (game.Promotee is not null)
            {
                PieceType? newType = null;
                var promoted = false;
                print(null);
                Console.WriteLine($"Pawn needs to be promoted. Press R for rook, N for knight, Q for queen or B for biship");
                while (!promoted)
                {
                    var e = Console.ReadKey();
                    if (e.Key == ConsoleKey.R)
                    {
                        newType = PieceType.R;
                    }
                    if (e.Key == ConsoleKey.N)
                    {
                        newType = PieceType.N;
                    }
                    if (e.Key == ConsoleKey.Q)
                    {
                        newType = PieceType.Q;
                    }
                    if (e.Key == ConsoleKey.B)
                    {
                        newType = PieceType.B;
                    }
                    if (newType.HasValue)
                    {
                        game.PromotePiece(game.Promotee, newType.Value);
                        promoted = true;
                    }
                    else
                    {
                        Console.WriteLine($"-- invalid piece: {e.Key}. Must be either of R/N/Q/B. Try again.");
                    }
                }
            }
            game.SwitchTurns();
            print(msg);

        }

    }
}


//todo 

// checkmate
// pawn promotion
// castling