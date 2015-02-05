using System;
using System.Globalization;
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
    internal class GameScoreConsole : Console
    {
        public event EventHandler RestartGame;
        public event EventHandler QuitGame;
        private int releaseCount;
        private bool playAgain;
        private Collection<CastleItem> scoringItems;
        private Collection<Monster> scoringMonsters;
        private bool win;

        public GameScoreConsole(CastleConsole castleConsole) : base(40, 25)
        {
            if(castleConsole.GameResult == GameResult.Win)
            {
                win = true;
            }
            else
            {
                win = false;
            }

            releaseCount = 2;
            playAgain = false;
            PrintResult(castleConsole.GameResult);
            PrintTreasures(castleConsole);
            PrintMonsters(castleConsole);
            PrintScore();
            this.CellData.Print(11, 24, CastleAdventureResources.PressAnyKey2, CastleGame.GameColor);
        }

        private void PrintResult(GameResult result)
        {
            switch(result)
            {
                case GameResult.Quit:
                    this.CellData.Print(8, 1, CastleAdventureResources.QuitGame, CastleGame.GameColor);
                    break;
                case GameResult.Failed:
                    this.CellData.Print(5, 1, CastleAdventureResources.FailedGame, CastleGame.GameColor);
                    break;
                case GameResult.Win:
                    this.CellData.Print(5, 1, CastleAdventureResources.EscapedCastle, CastleGame.GameColor);
                    break;
            }
        }

        private void PrintTreasures(CastleConsole castleConsole)
        {
            this.CellData.Print(1, 5, CastleAdventureResources.CollectedTreasures, CastleGame.GameColor);

            scoringItems = new Collection<CastleItem>();
            foreach(var item in castleConsole.ItemManager.CastleItems)
            {
                if(item.Collected)
                {
                    if(item.Value > 0)
                    {
                        scoringItems.Add(item);
                    }
                }
            }

            if(scoringItems.Count == 0)
            {
                this.CellData.Print(17, 7, CastleAdventureResources.None, CastleGame.GameColor);
            }
            else
            {
                int xPoint = 19;
                xPoint = xPoint -= scoringItems.Count;
                foreach (var item in this.scoringItems)
                {
                    this.CellData.SetCharacter(xPoint, 7, item.Character, CastleGame.GameColor);
                    xPoint += 2;
                }
            }

        }

        private void PrintMonsters(CastleConsole castleConsole)
        {
            this.CellData.Print(1, 11, CastleAdventureResources.KilledMonsters, CastleGame.GameColor);
            scoringMonsters = new Collection<Monster>();

            foreach(Monster monster in castleConsole.ItemManager.CastleMonsters)
            {
                if(monster.IsAlive == false)
                {
                    if(monster.Value > 0)
                    {
                        scoringMonsters.Add(monster);
                    }
                }
            }
            
            if (scoringMonsters.Count == 0)
            {
                this.CellData.Print(17, 13, CastleAdventureResources.None, CastleGame.GameColor);
            }
            else
            {
                int xPoint = 19;
                xPoint = xPoint -= scoringMonsters.Count;
                foreach (var item in this.scoringMonsters)
                {
                    this.CellData.SetCharacter(xPoint, 13, item.Character, CastleGame.GameColor);
                    xPoint += 2;
                }
            }
        }

        private void PrintScore()
        {
            int score = 0;
            foreach (var item in this.scoringItems)
            {
                score += item.Value;
            }
            foreach (var item in this.scoringMonsters)
            {
                score += item.Value;
            }
            if (win)
            {
                score += 100;
            }
            this.CellData.Print(12, 20, String.Format(CultureInfo.CurrentCulture, CastleAdventureResources.YourScore, score), CastleGame.GameColor);
            this.CellData.Print(11, 21, CastleAdventureResources.PerfectScore, CastleGame.GameColor);
        }

        public override bool ProcessKeyboard(SadConsole.Input.KeyboardInfo info)
        {
            if(info == null)
            {
                return false;
            }
            if(info.KeysReleased.Count > 0)
            {
                if (releaseCount > 0)
                {
                    releaseCount--;
                    return true;
                }
                if (playAgain == false)
                {
                    // Clear Line
                    for (int x = 8; x < 39; x++ )
                    {
                        this.CellData.SetCharacter(x, 24, 0, CastleGame.GameColor);
                    }
                    this.CellData.Print(8, 24, CastleAdventureResources.PlayAgain, CastleGame.GameColor);
                    playAgain = true;
                    return true;
                }
                if(info.IsKeyReleased(Keys.Y))
                {
                    if (RestartGame != null)
                    {
                        RestartGame(this, new EventArgs());
                    }
                    return true;
                }

                if (info.IsKeyReleased(Keys.N))
                {
                    if (QuitGame != null)
                    {
                        QuitGame(this, new EventArgs());
                    }
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

    }
}
