﻿using System;
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
    internal enum GameResult
    {
        Quit,
        Failed,
        Win

    }
    internal class CastleConsole : Console
    {
        private const int MaxTurnCount = 3;
        private const int MonsterTurn = 2;


        public GameResult GameResult { get; private set; }
        public event EventHandler StopGamePlay;

        public ItemManager ItemManager { get; private set; }

        private const int maxItems = 6;
        private const string prompt = "?";
        private ClassicConsoleKeyboardHandler keyboardHandlerObject;

        private Player player;
        private MapReader mapReader;
        private int currentRoomIndex;
        private Room currentRoom;
        public bool RunTick { get; set; }
        private bool gameOver;
        private bool firstRelease;

        private int waterXStart;
        private int waterXEnd;
        private int waterYStart;
        private int waterYEnd;
        private InstructionSet animation = null;
        private int currentTurnCount;



        public CastleConsole() : base(40, 25)
        {
            currentTurnCount = 0;
            this.mapReader = new MapReader();
            GameResult = GameResult.Quit;
            ItemManager = new ItemManager(UpdateMap);
            this.gameOver = false;
            this.IsVisible = false;
            this.firstRelease = true;
            RunTick = false;
            keyboardHandlerObject = new ClassicConsoleKeyboardHandler();
            VirtualCursor.Position = new Point(Room.MapWidth + 1, Room.MapHeight + 4);
            KeyboardHandler = keyboardHandlerObject.HandleKeyboard;

            keyboardHandlerObject.EnterPressedAction = EnterPressedActionHandler;

            // Enable the keyboard and setup the prompt.
            CanUseKeyboard = true;
            VirtualCursor.IsVisible = true;
            // Startup description
            CellData.Clear();
            _cellData.TimesShiftedUp = 0;
            VirtualCursor.Print(CastleAdventureResources.CommandPrompt);


            currentRoomIndex = 1;
            player = new Player();
            player.Position = new Microsoft.Xna.Framework.Point(_cellData.Width / 2, _cellData.Height / 2);



            DrawBorder();
            DrawRoom();

        }


        public override bool ProcessKeyboard(SadConsole.Input.KeyboardInfo info)
        {
            if(info == null)
            {
                return false;
            }
            if (gameOver == false)
            {
                if (player.ProcessKeyboard(info))
                {

                    return true;
                }
                else
                {
                    base.ProcessKeyboard(info);
                }
                return false;
            }
            else
            {
                if (info.KeysReleased.Count > 0)
                {
                    if (firstRelease)
                    {
                        firstRelease = false;
                        return true;
                    }
                    else
                    {
                        if (StopGamePlay != null)
                        {
                            StopGamePlay(this, new EventArgs());
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
                
            }
            
        }
        public override void Update()
        {
            if (RunTick && player.Health > 0)
            {
                currentTurnCount++;
                if (currentTurnCount == MaxTurnCount)
                {
                    currentTurnCount = 0;
                }

                RunTick = false;
                MovePlayer();
                player.Update();
                foreach (Monster monster in ItemManager.CurrentRoomMonsters)
                {
                    if (currentTurnCount == MonsterTurn)
                    {
                        if (monster.IsAlive)
                        {
                            MoveMonster(monster);
                        }
                    }
                    monster.Update();
                }


            }
            base.Update();

            if (animation != null)
            {
                animation.Run();
            }
        }
        protected override void OnAfterRender()
        {
            player.Render();
            foreach (Monster monster in ItemManager.CurrentRoomMonsters)
            {
                monster.Render();
            }
            base.OnAfterRender();
        }

        private void MovePlayer()
        {
            UserMessage message;

            int testRoomIndex;
            Point previewMove = player.PreviewMove(player.CurrentDirection);
            switch(CollisionDetection(previewMove))
            {
                case ObjectType.None:
                    player.Move(previewMove);
                    break;

                case ObjectType.Door:
                    if (currentRoom.RoomWarp == null)
                    {
                        testRoomIndex = currentRoom.GetNextRoom(player.CurrentDirection);
                        if(testRoomIndex == -1)
                        {
                            this.GameResult = Castle.GameResult.Win;
                            EndGame();
                            if (StopGamePlay != null)
                            {
                                StopGamePlay(this, new EventArgs());
                            }
                            return;
                        }
                        if (mapReader.FindRoom(testRoomIndex).Dark)
                        {
                            if (ItemManager.FindItemInInventory("Lamp") != null)
                            {
                                currentRoomIndex = testRoomIndex;
                                MovePlayerRoom(previewMove);
                                DrawRoom();
                            }
                            else
                            {
                                message = new UserMessage();
                                message.AddLine("It's too dark");
                                message.AddLine("to go that way");
                                PrintUserMessage(message);
                            }
                        }
                        else
                        {
                            currentRoomIndex = testRoomIndex;
                            MovePlayerRoom(previewMove);
                            DrawRoom();
                        }
                    }
                    else
                    {
                        currentRoomIndex = currentRoom.RoomWarp.DestinationRoomId;
                        previewMove.X = currentRoom.RoomWarp.X;
                        previewMove.Y = currentRoom.RoomWarp.Y;
                        message = currentRoom.RoomWarp.UserMessage;
                        player.Move(previewMove);
                        DrawRoom();
                        PrintUserMessage(message);
                    }
                    break;

                case ObjectType.UpStairs:
                    currentRoomIndex = currentRoom.GetNextRoom(Direction.UpStairs);
                    DrawRoom();
                    break;

                case ObjectType.DownStairs:
                    testRoomIndex = currentRoom.GetNextRoom(Direction.DownStairs);
                    if (mapReader.FindRoom(testRoomIndex).Dark)
                    {
                        if (ItemManager.FindItemInInventory("Lamp") != null)
                        {
                            currentRoomIndex = currentRoom.GetNextRoom(Direction.DownStairs);
                            DrawRoom();
                        }
                        else
                        {
                            message = new UserMessage();
                            message.AddLine("It's too dark");
                            message.AddLine("to go that way");
                            PrintUserMessage(message);
                        }
                    }
                    else
                    {
                        currentRoomIndex = currentRoom.GetNextRoom(Direction.DownStairs);
                        DrawRoom();
                    }
                    break;

                case ObjectType.LockedDoor:
                            message = new UserMessage();
                            message.AddLine("The Door is");
                            message.AddLine("locked");
                            PrintUserMessage(message);
                    break;

                case ObjectType.Trap:
                    if(ProcessTrap() == false)
                    {
                        player.Move(previewMove);
                    }
                    break;
                case ObjectType.Item:
                    PickupItem(previewMove);
                    break;

                case ObjectType.Monster:
                    AttackMonster(previewMove);
                    break;
            }
        }
        private void MoveMonster(Monster monster)
        {

            Point previewMove = monster.PreviewMove(player.Position);

            if (previewMove.X == player.Position.X && previewMove.Y == player.Position.Y)
            {
                UserMessage message = new UserMessage();
                bool hasHelmet = false;
                CastleItem helmet = ItemManager.FindItemInInventory("helmet");
                if(helmet != null)
                {
                    hasHelmet = true;
                }

                if (player.Hit(hasHelmet))
                {
                    message.AddLine(String.Format(CultureInfo.CurrentCulture, "The {0}", monster.Name));
                    message.AddLine("killed you!");
                    PrintUserMessage(message);
                    GameResult = GameResult.Failed;
                    EndGame();
                    gameOver = true;
                }
                else
                {
                    message.AddLine(String.Format(CultureInfo.CurrentCulture, "The {0}", monster.Name));
                    message.AddLine("struck you!");
                    if (hasHelmet)
                    {
                        message.AddLine(" ");
                    }
                    message.AddLine("The Helmet");
                    message.AddLine("helped.");
                    PrintUserMessage(message);
                }
            }
            else
            {
                switch (CollisionDetection(previewMove))
                {
                    case ObjectType.None:
                        monster.Move(previewMove);
                        break;
                }
            }
        }

        private void PickupItem(Point item)
        {
            PrintUserMessage(ItemManager.PickupItem(item));
            if (ItemManager.IsItemAtPoint(item) == false)
            {
                this.CellData.SetCharacter(item.X, item.Y, 0, CastleGame.GameColor);
            }
        }
        private void AttackMonster(Point monster)
        {
            PrintUserMessage(ItemManager.AttackMonster(monster));
            
        }
        private void MovePlayerRoom(Point previewMove)
        {
            switch(player.CurrentDirection)
            {
                case Direction.Up:
                    previewMove.Y = Room.MapHeight - 1;
                    break;
                case Direction.Down:
                    previewMove.Y = 0;
                    break;
                case Direction.Left:
                    previewMove.X = Room.MapWidth - 1;
                    break;
                case Direction.Right:
                    previewMove.X = 0;
                    break;
            }
            player.Move(previewMove);
        }

        private ObjectType CollisionDetection(Point previewMove)
        {
            int value = currentRoom.GetMapPoint(previewMove.X, previewMove.Y);

            ObjectType returnValue = ObjectType.None;

            if(value == 32)
            {
                returnValue = ObjectType.None;
            }
            else if (value == 85)
            {
                returnValue = ObjectType.UpStairs;
            }
            else if (value == 68)
            {
                returnValue = ObjectType.DownStairs;
            }
            else if (value == 8)
            {
                returnValue = ObjectType.LockedDoor;
            }
            else if(value == -1)
            {
                returnValue = ObjectType.Door;
            }
            else if(value == -2)
            {
                returnValue = ObjectType.Trap;
            }
            else
            {
                returnValue = ObjectType.Wall;
            }

            if(returnValue == ObjectType.None)
            {
                if (ItemManager.IsItemAtPoint(previewMove))
                {
                    returnValue = ObjectType.Item;
                }
                else
                {
                    if(ItemManager.IsMonsterAtPoint(previewMove))
                    {
                        returnValue = ObjectType.Monster;
                    }
                }
            }


            return returnValue;
        }


        private void DrawRoom()
        {
            if (currentRoom != null)
            {
                currentRoom.ClearReplacePoints();
            }
            currentRoom = mapReader.FindRoom(currentRoomIndex);
            ItemManager.LoadRoomItems(currentRoomIndex, player.Position);
            DrawMap();
            DrawDescription();
            ResetPrompt();
            ClearMessages(true);
            ClearItemList();
            DrawItems();
            
        }

        private void DrawItems()
        {
            int y = 0;
            foreach (CastleItem castleItem in ItemManager.CurrentRoomItems)
            {
                this.CellData.SetCharacter(castleItem.Location.X, castleItem.Location.Y, castleItem.Character, CastleGame.GameColor);

                this.CellData.SetCharacter(Room.MapWidth + 1, y, castleItem.Character, CastleGame.GameColor);
                this.CellData.Print(Room.MapWidth + 3, y, castleItem.InventoryName, CastleGame.GameColor);
                y++;

            }

            foreach (Monster monster in ItemManager.CurrentRoomMonsters)
            {
                monster.IsVisible = true;

                this.CellData.SetCharacter(Room.MapWidth + 1, y, monster.Character, CastleGame.GameColor);
                string monsterName = monster.InventoryName;
                if(monster.IsAlive == false)
                {
                    monsterName = String.Format(CultureInfo.CurrentCulture, CastleAdventureResources.MosterDead, monster.Name);
                }
                this.CellData.Print(Room.MapWidth + 3, y, monsterName, CastleGame.GameColor);
                y++;
            }

        }

        private void DrawMap()
        {
            // Draw from File
            for (int indexX = 0; indexX < Room.MapWidth; indexX++)
            {
                for(int indexY = 0; indexY < Room.MapHeight; indexY++)
                {
                    int mapPoint = currentRoom.GetMapPoint(indexX, indexY);
                    if(mapPoint == -2)
                    {
                        mapPoint = 0;
                    }
                    this.CellData.SetCharacter(indexX, indexY, mapPoint, CastleGame.GameColor);
                }
            }

            if(currentRoom.Door != null)
            {
                if(currentRoom.Door.Locked)
                {
                    foreach(Point doorPoint in currentRoom.Door.DoorPoints)
                    {
                        this.CellData.SetCharacter(doorPoint.X, doorPoint.Y, 8, CastleGame.GameColor);
                    }
                }
            }

        }
        private void UpdateMap(Collection<ReplacementPoint> replacementPoints)
        {
            currentRoom.ReplaceMapPoint(replacementPoints);
            foreach(var point in replacementPoints)
            {
                this.CellData.SetCharacter(point.X, point.Y, point.Character, CastleGame.GameColor);
            }
            
        }

        private void DrawDescription()
        {
            for (int index = 0; index < Room.DescriptionHeight; index++)
            {
                this.CellData.Print(0, Room.MapHeight + index + 1, currentRoom.GetDescriptionLine(index), CastleGame.GameColor);
            }
        }
        private void DrawBorder()
        {
            for (int x = 0; x < Room.MapWidth; x++)
            {
                this.CellData.SetCharacter(x, Room.MapHeight, 196, CastleGame.GameColor);
            }

            this.CellData.SetCharacter(Room.MapWidth, Room.MapHeight, 217, CastleGame.GameColor);

            for (int y = 0; y < Room.MapHeight; y++)
            {
                this.CellData.SetCharacter(Room.MapWidth, y, 179, CastleGame.GameColor);
            }
        }


        private void EnterPressedActionHandler(string value)
        {
            ParsePrimaryCommands(value);
            ResetPrompt();
        }

        private void ResetPrompt()
        {
            // Reset the command
            for (int x = Room.MapWidth + 2; x < Room.MapWidth + 16; x++)
            {
                this.CellData.SetCharacter(x, Room.MapHeight + 4, 0, CastleGame.GameColor);
            }
            VirtualCursor.Position = new Point(Room.MapWidth + 1, Room.MapHeight + 4);
            VirtualCursor.Print(CastleAdventureResources.CommandPrompt);

            keyboardHandlerObject.VirtualCursorLastY = Room.MapHeight + 4;
        }
        private void ClearItemList()
        {
            for (int x = Room.MapWidth + 1; x < Room.MapWidth + 14; x++)
            {
                for (int y = 0; y < Room.MapHeight - 5; y++)
                {
                    this.CellData.SetCharacter(x, y, 0, CastleGame.GameColor);
                }
            }
        }
        private void ClearMessages(bool clearAll)
        {
            int yStart;
            if(clearAll)
            {
                yStart = 0;
            }
            else
            {
                yStart = 8;
            }
            for (int x = Room.MapWidth + 1; x < Room.MapWidth + 16; x++)
            {
                for (int y = yStart; y < Room.MapHeight + 3; y++)
                {
                    this.CellData.SetCharacter(x, y, 0, CastleGame.GameColor);
                }
                this.CellData.SetCharacter(x, Room.MapHeight + 5, 0, CastleGame.GameColor);
            }
        }

        private void ParsePrimaryCommands(string commands)
        {
            ClearMessages(false);

            Command command = Command.Parse(commands);
            
            switch(command.Verb)
            {
                case CommandVerb.Inventory:
                    ShowInventory();
                    break;
                case CommandVerb.Look:
                    ProcessLook(command);
                    break;
                case CommandVerb.Take:
                    ProcessTake(command);
                    break;
                case CommandVerb.Drop:
                    ProcessDropItem(command);
                    break;
                case CommandVerb.Drink:
                    ProcessDrink(command);
                    break;
                case CommandVerb.Wear:
                    ProcessWear(command);
                    break;
                case CommandVerb.Read:
                    ProcessRead(command);
                    break;
                case CommandVerb.Wave:
                    ProcessWave(command);
                    break;
                case CommandVerb.Open:
                    ProcessOpen(command);
                    break;
                case CommandVerb.Show:
                    ProcessShow(command);
                    break;
                case CommandVerb.Play:
                    ProcessPlay(command);
                    break;
                case CommandVerb.Quit:
                    ProcessQuit();
                    break;
                default:
                    ShowError();
                    break;
            }
        }

        private void PrintUserMessage(UserMessage userMessage)
        {
            ClearMessages(false);
            int index = Room.MapHeight - 5;
            foreach (string line in userMessage.Messages)
            {
                this.CellData.Print(Room.MapWidth + 1, index, line, CastleGame.GameColor);
                index++;
            }
        }
        private void ShowInventory()
        {
            PrintUserMessage(this.ItemManager.CommandInventory());
        }

        private void ProcessLook(Command command)
        {
            PrintUserMessage(this.ItemManager.CommandLook(command));
        }

        private void ProcessTake(Command command)
        {
            PrintUserMessage(this.ItemManager.CommandTake(command, player.Position));
        }

        private void ProcessDropItem(Command command)
        {
            Point dropCoordinates = player.GetFacingPoint();
            bool collision;
            if (CollisionDetection(dropCoordinates) == ObjectType.None)
            {
                collision = false;
            }
            else
            {
                collision = true;
            }

            UserMessage message = this.ItemManager.CommandDrop(command, dropCoordinates, collision);
            if(message.Character.HasValue)
            {
                this.CellData.SetCharacter(dropCoordinates.X, dropCoordinates.Y, message.Character.Value, CastleGame.GameColor);
            }
            PrintUserMessage(message);

        }
        private void ProcessDrink(Command command)
        {
            PrintUserMessage(ItemManager.CommandDrink(command, player.Position));
        }
        private void ProcessWear(Command command)
        {
            PrintUserMessage(ItemManager.CommandWear(command, player.Position));

        }
        private void ProcessRead(Command command)
        {
            PrintUserMessage(ItemManager.CommandRead(command, player.Position));
        }
        private void ProcessWave(Command command)
        {
            PrintUserMessage(ItemManager.CommandWave(command, player.Position));
        }
        private void ProcessOpen(Command command)
        {
            bool priorLock = false;

            if(currentRoom.Door != null)
            {
                priorLock = currentRoom.Door.Locked;
            }


            PrintUserMessage(ItemManager.CommandOpen(command, player.Position, currentRoom));

            if(priorLock == true)
            {
                if(currentRoom.Door != null)
                {
                    if(currentRoom.Door.Locked == false)
                    {
                        // Erase Lock
                        foreach(Point doorPoint in currentRoom.Door.DoorPoints)
                        {
                            this.CellData.SetCharacter(doorPoint.X, doorPoint.Y, 0, CastleGame.GameColor);
                        }
                    }
                }
            }

            
        }
        private void ProcessShow(Command command)
        {
            PrintUserMessage(ItemManager.CommandShow(command));
        }
        private void ProcessPlay(Command command)
        {
            PrintUserMessage(ItemManager.CommandPlay(command, player.Position));
        }
        private void ProcessQuit()
        {
            GameResult = GameResult.Quit;
            EndGame();
            if (StopGamePlay != null)
            {
                StopGamePlay(this, new EventArgs());
            }
        }
        private bool ProcessTrap()
        {
            if(ItemManager.FindItemInInventory("Necklace") != null)
            {
                return false;
            }

            GameResult = GameResult.Failed;

            // Draw Walls
            foreach(Point wallPoint in currentRoom.Trap.WallPoints)
            {
                this.CellData.SetCharacter(wallPoint.X, wallPoint.Y, 178, CastleGame.GameColor);
            }

            waterXStart = currentRoom.Trap.WaterFlowXStart;
            waterXEnd = currentRoom.Trap.WaterFlowXEnd - 1;
            waterYStart = currentRoom.Trap.WaterFlowYStart;
            waterYEnd = currentRoom.Trap.WaterFlowYEnd;

            // Animate the water
            animation = new InstructionSet();
            animation.Instructions.AddLast(new Wait() { Duration = 0.2f });

            var waterFillInstruction = new CodeInstruction();
            waterFillInstruction.CodeCallback = (inst) =>
            {
                waterXStart += 1;

                if (waterXStart > waterXEnd)
                {
                    inst.IsFinished = true;
                    gameOver = true;
                }
                this.CellData.SetCharacter(waterXStart, waterYStart, 176, CastleGame.GameColor);
                this.CellData.SetCharacter(waterXStart, waterYEnd, 176, CastleGame.GameColor);
            };
            animation.Instructions.AddLast(waterFillInstruction);

            UserMessage userMessage = new UserMessage();
            userMessage.AddLine("You have");
            userMessage.AddLine("sprung a Trap!");
            userMessage.AddLine("The room has");
            userMessage.AddLine("filled with");
            userMessage.AddLine("water and you");
            userMessage.AddLine("drowned!");

            // Kill Character
            this.player.Kill();
            PrintUserMessage(userMessage);
            EndGame();

            return true;
        }

        private void ShowError()
        {
            this.CellData.Print(Room.MapWidth + 3, Room.MapHeight + 5, CastleAdventureResources.CommandError, CastleGame.GameColor);
        }
        private void EndGame()
        {
            VirtualCursor.IsVisible = false;


            for (int x = 0; x < 40 ; x++)
            {
                for (int y = Room.MapHeight + 1; y < 25; y++)
                {
                    this.CellData.SetCharacter(x, y, 0, CastleGame.GameColor);
                }
            }

            this.CellData.Print(8, 24, CastleAdventureResources.PressAnyKey, CastleGame.GameColor);

        }


    }
}
