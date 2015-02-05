﻿using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SadConsole.Consoles;
using SadConsole.Input;


namespace Castle
{
    internal class ClassicConsoleKeyboardHandler
    {
        private const string prompt = "?";

        // This holds the row that the virtual cursor is starting from when someone is typing.
        public int VirtualCursorLastY;

        // this is a callback for the owner of this keyboard handler. It is called when the user presses ENTER.
        public Action<string> EnterPressedAction;

        public bool HandleKeyboard(IConsole console, KeyboardInfo info)
        {
            // Check each key pressed.
            foreach (var key in info.KeysPressed)
            {
                // If the character associated with the key pressed is a printable character, print it
                if (key.Character != '\0')
                {
                    int startingIndex = console.CellData.GetIndexFromPoint(new Point(Room.MapWidth + 2, Room.MapHeight + 4));
                    String data = console.CellData.GetString(startingIndex, console.CellData.GetIndexFromPoint(console.VirtualCursor.Position) - startingIndex);
                    if (data.Length < 14)
                    {
                        console.VirtualCursor.Print(key.Character.ToString().ToUpper(CultureInfo.CurrentCulture));
                    }
                }

                // Special character - BACKSPACE
                else if (key.XnaKey == Keys.Back)
                {

                    // If the console has scrolled since the user started typing, adjust the starting row of the virtual cursor by that much.
                    if (console.CellData.TimesShiftedUp != 0)
                    {
                        console.CellData.TimesShiftedUp = 0;
                    }

                    // Do not let them backspace into the prompt
                    if (console.VirtualCursor.Position.Y != Room.MapHeight + 4 || console.VirtualCursor.Position.X > Room.MapWidth + 2)
                    {
                        console.VirtualCursor.LeftWrap(1).Print(CastleAdventureResources.EmptySpace).LeftWrap(1);
                    }
                }

                // Special character - ENTER
                else if (key.XnaKey == Keys.Enter)
                {
                    // If the console has scrolled since the user started typing, adjust the starting row of the virtual cursor by that much.
                    if (console.CellData.TimesShiftedUp != 0)
                    {
                        VirtualCursorLastY -= console.CellData.TimesShiftedUp;
                        console.CellData.TimesShiftedUp = 0;
                    }

                    // Get the prompt to exclude it in determining the total length of the string the user has typed.
                    int startingIndex = console.CellData.GetIndexFromPoint(new Point(Room.MapWidth + 2, Room.MapHeight + 4));
                    String data = console.CellData.GetString(startingIndex, console.CellData.GetIndexFromPoint(console.VirtualCursor.Position) - startingIndex);

                    // Move the cursor to the next line before we send the string data to the processor

                    // Send the string data
                    EnterPressedAction(data);

                    // After they have processed the string, we will create a new line and display the prompt.
                    VirtualCursorLastY = console.VirtualCursor.Position.Y;

                    // Preparing the next lines could have scrolled the console, reset the counter
                    console.CellData.TimesShiftedUp = 0;
                }
            }

            return true;
        }
    }
}
