using System;
using System.Text;
using SadConsole;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SadConsole.Consoles;
using Console = SadConsole.Consoles.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SadConsole.Instructions;

namespace Castle
{
    internal class GameMenuConsole : Console
    {
        public event EventHandler PlayGame;
        public GameMenuConsole()
            : base(40, 25)
        {
            DrawTitle();
            DrawCastle();
            DrawCommands();
        }

        private void DrawTitle()
        {
            this.CellData.Print(11, 1, CastleAdventureResources.CastleAdventureTitle, CastleGame.GameColor);
        }

        private void DrawCastle()
        {
            // Draw Tower 1
            for (int x = 4; x < 11; x++)
            {
                this.CellData.SetCharacter(x, 5, 177, CastleGame.GameColor);
            }
            for (int x = 5; x < 10; x++)
            {
                for (int y = 6; y < 8; y++)
                {
                    this.CellData.SetCharacter(x, y, 177, CastleGame.GameColor);
                }
            }
            this.CellData.SetCharacter(4, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(6, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(8, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(10, 4, 219, CastleGame.GameColor);

            // Draw Tower 2
            for (int x = 28; x < 35; x++)
            {
                this.CellData.SetCharacter(x, 5, 177, CastleGame.GameColor);
            }
            for (int x = 29; x < 34; x++)
            {
                for (int y = 6; y < 8; y++)
                {
                    this.CellData.SetCharacter(x, y, 177, CastleGame.GameColor);
                }
            }
            this.CellData.SetCharacter(28, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(30, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(32, 4, 219, CastleGame.GameColor);
            this.CellData.SetCharacter(34, 4, 219, CastleGame.GameColor);

            // Draw tops of walls
            this.CellData.SetCharacter(11, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(12, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(14, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(15, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(17, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(18, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(20, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(21, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(23, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(24, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(26, 7, 220, CastleGame.GameColor);
            this.CellData.SetCharacter(27, 7, 220, CastleGame.GameColor);

            // Draw Walls
            for (int x = 5; x < 34; x++)
            {
                for (int y = 8; y < 11; y++)
                {
                    this.CellData.SetCharacter(x, y, 177, CastleGame.GameColor);
                }
            }
            for (int x = 7; x < 32; x++)
            {
                for (int y = 11; y <= 21; y++)
                {
                    this.CellData.SetCharacter(x, y, 177, CastleGame.GameColor);
                }
            }

            // Draw Gate Wall
            for (int x = 17; x < 22; x++)
            {
                for (int y = 17; y <= 21; y++)
                {
                    this.CellData.SetCharacter(x, y, 219, CastleGame.GameColor);
                }
            }


            // Draw Gate
            for (int x = 18; x < 21; x++)
            {
                for (int y = 18; y <= 21; y++)
                {
                    this.CellData.SetCharacter(x, y, 197, CastleGame.GameColor);
                }
            }
        }

        private void DrawCommands()
        {
            for (int x = 0; x < 39; x++)
            {
                this.CellData.SetCharacter(x, 22, 196, CastleGame.GameColor);
            }
            this.CellData.Print(2, 24, CastleAdventureResources.PToPlay, CastleGame.GameColor);
            this.CellData.Print(17, 24, CastleAdventureResources.IForInstructions, CastleGame.GameColor);
        }

        public override bool ProcessKeyboard(SadConsole.Input.KeyboardInfo info)
        {
            if(info == null)
            {
                return false;
            }
            if(info.IsKeyReleased(Keys.P))
            {
                if (PlayGame != null)
                {
                    PlayGame(this, new EventArgs());
                }
                return true;
            }
            if (info.IsKeyReleased(Keys.I))
            {
                PrintInstructions();
                return true;
            }
            return false;
        }

        private void PrintInstructions()
        {
            // Clear screen
            for (int x = 0; x < 40; x++)
            {
                for (int y = 0; y < 23; y++)
                {
                    this.CellData.SetCharacter(x, y, 0, CastleGame.GameColor);
                }
            }
            this.CellData.Print(0, 2, CastleAdventureResources.Intro1, CastleGame.GameColor);
            this.CellData.Print(0, 3, CastleAdventureResources.Intro2, CastleGame.GameColor);
            this.CellData.Print(0, 4, CastleAdventureResources.Intro3, CastleGame.GameColor);
            this.CellData.Print(0, 5, CastleAdventureResources.Intro4, CastleGame.GameColor);
            this.CellData.Print(0, 7, CastleAdventureResources.Intro5, CastleGame.GameColor);
            this.CellData.Print(0, 8, CastleAdventureResources.Intro6, CastleGame.GameColor);
            this.CellData.Print(0, 9, CastleAdventureResources.Intro7, CastleGame.GameColor);
            this.CellData.Print(0, 10, CastleAdventureResources.Intro8, CastleGame.GameColor);
            this.CellData.Print(0, 11, CastleAdventureResources.Intro9, CastleGame.GameColor);
            this.CellData.Print(0, 12, CastleAdventureResources.Intro10, CastleGame.GameColor);
            this.CellData.Print(0, 13, CastleAdventureResources.Intro11, CastleGame.GameColor);
            this.CellData.Print(0, 14, CastleAdventureResources.Intro12, CastleGame.GameColor);
            this.CellData.Print(0, 15, CastleAdventureResources.Intro13, CastleGame.GameColor);
            this.CellData.Print(0, 18, CastleAdventureResources.Intro14, CastleGame.GameColor);
        }
    }
}

