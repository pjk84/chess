using Chess.Models;
using Mindmagma.Curses;

using Chess.Interfaces;
class Demo
{

    static void Main()
    {
        string? inp = null;
        string? msg = null;
        IChessGame game = new Game();

        void print(string? msg, string? instruction)
        {
            game.PrintBoard(msg);

        }


        // Console.WriteLine("commands:");
        // Console.WriteLine("moves  --> show sequence of moves");
        // Console.WriteLine("save  --> save game");
        // Console.WriteLine("load  --> load game");
        // Console.WriteLine("reset --> reset game");
        // Console.WriteLine("exit  --> quit game");
        // Console.WriteLine(game.PrintBoard());
        print(null, null);
        System.ConsoleKey[] arrowKeys = { ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.UpArrow, ConsoleKey.DownArrow };
        while (game.IsPlaying)
        {
            var e = Console.ReadKey();
            if (arrowKeys.Contains(e.Key))
            {
                game.SetCursor(e.Key);
            }
            if (e.Key == ConsoleKey.Spacebar)
            {
                try
                {

                    msg = null;
                    if (game.PieceSelectedAt is not null)
                    {
                        game.MovePiece();
                        game.SwitchTurns();
                    }
                    else
                    {
                        game.SelectPiece();
                    }
                }
                catch (Exception err)
                {
                    msg = $"illegal move: {err.Message}";
                }
            }
            if (e.Key == ConsoleKey.Escape)
            {
                if (game.PieceSelectedAt is not null)
                {
                    game.ReturnPiece();
                }

            }
            if (e.Key == ConsoleKey.Backspace)
            {
                try
                {

                    game.UndoAction();
                }
                catch (Exception err)
                {
                    msg = err.Message;
                }
            }

            //     msg = null;
            //     if (game.Promotee is not null)
            //     {
            //         PieceType? newType = null;
            //         var promoted = false;
            //         print(null, null);
            //         Console.WriteLine($"Pawn needs to be promoted. Press R for rook, N for knight, Q for queen or B for biship");
            //         while (!promoted)
            //         {
            //             var e = Console.ReadKey();
            //             if (e.Key == ConsoleKey.R)
            //             {
            //                 newType = PieceType.R;
            //             }
            //             if (e.Key == ConsoleKey.N)
            //             {
            //                 newType = PieceType.N;
            //             }
            //             if (e.Key == ConsoleKey.Q)
            //             {
            //                 newType = PieceType.Q;
            //             }
            //             if (e.Key == ConsoleKey.B)
            //             {
            //                 newType = PieceType.B;
            //             }
            //             if (newType.HasValue)
            //             {
            //                 game.Actions.Add(new Action(game.Promotee, null, null, new Promotion(game.Promotee.Type, newType.Value), false));
            //                 game.Promotee.Type = newType.Value;
            //                 promoted = true;
            //             }
            //             else
            //             {
            //                 Console.WriteLine($"-- invalid piece: {e.Key}. Must be either of R/N/Q/B. Try again.");
            //             }
            //         }
            //         print($"Promoted to {newType}", null);
            //         game.Promotee = null;
            //         continue;
            //     }
            //     // Console.Write("make a move: ");
            //     inp = Console.ReadLine()?.ToLower();
            //     if (inp is null)
            //     {
            //         continue;
            //     }
            //     if (inp == "castle")
            //     {
            //         Console.WriteLine("which R do you want to castle?");
            //         string? address = Console.ReadLine()?.ToLower();
            //         if (address is null)
            //         {
            //             continue;
            //         }
            //         try
            //         {
            //             game.Castle(address);
            //             print($"castled R {address}", null);
            //         }
            //         catch (Exception e)
            //         {
            //             Console.WriteLine(e);
            //             continue;
            //         }

            //         continue;
            //     }
            //     if (inp == "load")
            //     {
            //         string[] files = Directory.GetFiles(@"games", "*.json");
            //         Console.WriteLine("\nfound games: \n");
            //         foreach (var file in files)
            //         {
            //             var f = file.Replace("games/", "").Replace(".json", "");
            //             Console.WriteLine($"- {f}");
            //         }
            //         Console.Write($"\nfound {files.Length} savegames. Which game do you want to continue? ");
            //         string? fileName = Console.ReadLine()?.ToLower();
            //         msg = $"succesfully loadded game {fileName}";
            //         if (fileName is null)
            //         {
            //             throw new Exception("-- invalid file name");
            //         }
            //         try
            //         {
            //             game.LoadGame(fileName);
            //         }
            //         catch (Exception e)
            //         {
            //             msg = $"could not load game: {e.Message}";
            //         }
            //         print(msg, null);
            //         continue;
            //     }
            //     if (inp == "moves")
            //     {
            //         if (game.Actions.Count == 0)
            //         {
            //             Console.WriteLine("no moves found..");
            //             continue;
            //         }
            //         var moves = game.PrintTurns();
            //         Console.WriteLine(moves);
            //         continue;
            //     }
            //     if (inp == "save")
            //     {
            //         Console.Write("enter filename. type 'skip' to continue without saving: ");
            //         string? fileName = Console.ReadLine();

            //         if (fileName is null)
            //         {
            //             throw new Exception("-- invalid file name");
            //         }
            //         if (fileName.ToLower() == "skip")
            //         {
            //             continue;
            //         }
            //         game.SaveGame(fileName);
            //         Console.WriteLine($"-- game saved as '{fileName}'. To load game use command 'load test'.");
            //         continue;
            //     }
            //     if (inp == "quit" || inp == "exit")
            //     {
            //         game.Quit();
            //         NCurses.Keypad(window, false);
            //         NCurses.NoCBreak();
            //         NCurses.UseDefaultColors();
            //         NCurses.EndWin();
            //         return;
            //     }
            //     if (inp == "restart" || inp == "reset")
            //     {
            //         game = new Game();
            //         print("restarted game!", null);
            //         continue;
            //     }
            //     if (inp == "undo")
            //     {
            //         if (game.Actions.Count() == 0)
            //         {
            //             Console.WriteLine("no turns found.. ");
            //             continue;
            //         }
            //         bool done = false;
            //         while (!done)
            //         {
            //             msg = $" found {game.Actions.Count()} move(s). Press delete/backspace to undo move. Press enter when done.";
            //             msg += game.PrintTurns();
            //             print(msg, null);
            //             var e = Console.ReadKey();
            //             if (e.Key == ConsoleKey.Enter)
            //             {
            //                 done = true;
            //                 continue;
            //             }
            //             if (e.Key == ConsoleKey.Backspace)
            //             {
            //                 game.UndoAction();
            //                 if (game.Actions.Count() == 0)
            //                 {
            //                     done = true;
            //                 }
            //             }

            //         }
            //         print(null, null);
            //         continue;
            //     }

            //     if (game.Checked is not null)
            //     {
            //         msg += $"player {game.Checked} is checked";
            //     }

            //     try
            //     {
            //         game.MakeMove(inp);
            //         game.SwitchTurns();
            //     }
            //     catch (MovementError e)
            //     {
            //         msg = $"move is invalid for piece of type {e.Type}. {e.Message}";
            //     }
            //     catch (CheckError e)
            //     {
            //         msg = $"{(e.Color == 0 ? "Kw" : "Kb")} checked by {e.Offender} at {e.Address} ";
            //     }
            //     catch (MoveParseError)
            //     {
            //         msg = "invalid move format. Move must be formatted as <from>-<to>. Example: a2-a3";
            //     }
            //     catch (AddressParseError e)
            //     {
            //         msg = $"invalid address '{e.Address}'";
            //     }
            //     catch (Exception e)
            //     {
            //         msg = $"{e.Message}";
            //     }
            print(msg, null);
        }

    }
}


//todo 
// checkmate
// undo castling move