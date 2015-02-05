﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = SadConsole.Consoles.Console;
using SadConsole.Entities;
using SadConsole.Consoles;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Castle
{
    internal class Player : Entity
    {
        public Direction CurrentDirection { get; private set; }
        public Direction Facing { get; private set; }
        public int Health { get; private set; }
        public Player()
        {
            _canUseKeyboard = true;
            _currentAnimation.CurrentFrame[0].CharacterIndex = 5;
            this.CurrentDirection = Direction.None;
            Health = 50;

        }

        public override bool ProcessKeyboard(SadConsole.Input.KeyboardInfo info)
        {
            if(info == null)
            {
                return false;
            }

            // Process logic for moving the entity.
            bool keyHit = false;

            if (info.IsKeyDown(Keys.Up))
            {
                this.CurrentDirection = Direction.Up;
                keyHit = true;
            }
            else if (info.IsKeyDown(Keys.Down))
            {
                this.CurrentDirection = Direction.Down;
                keyHit = true;
            }

            if (info.IsKeyDown(Keys.Left))
            {
                this.CurrentDirection = Direction.Left;
                keyHit = true;
            }
            else if (info.IsKeyDown(Keys.Right))
            {
                this.CurrentDirection = Direction.Right;
                keyHit = true;
            }

            switch(CurrentDirection)
            {
                case Direction.Up:
                    if(info.IsKeyReleased(Keys.Up))
                    {
                        this.CurrentDirection = Direction.None;
                        keyHit = true;
                    }
                    break;
                case Direction.Down:
                    if (info.IsKeyReleased(Keys.Down))
                    {
                        this.CurrentDirection = Direction.None;
                        keyHit = true;
                    }
                    break;
                case Direction.Left:
                    if (info.IsKeyReleased(Keys.Left))
                    {
                        this.CurrentDirection = Direction.None;
                        keyHit = true;
                    }
                    break;
                case Direction.Right:
                    if (info.IsKeyReleased(Keys.Right))
                    {
                        this.CurrentDirection = Direction.None;
                        keyHit = true;
                    }
                    break;
            }
            

            return keyHit;

        }
        public void Move(Point location)
        {
            if (this.IsVisible)
            {
                _position.X = location.X;
                _position.Y = location.Y;
            }
        }
        public Point PreviewMove(Direction direction)
        {
            Point preview = new Point(_position.X, _position.Y);

            switch (direction)
            {
                case Direction.Up:
                    this.Facing = Direction.Up;
                    preview.Y -= 1;
                    break;
                case Direction.Down:
                    this.Facing = Direction.Down;
                    preview.Y += 1;
                    break;
                case Direction.Left:
                    this.Facing = Direction.Left;
                    preview.X -= 1;
                    break;
                case Direction.Right:
                    this.Facing = Direction.Right;
                    preview.X += 1;
                    break;
            }

            return preview;
        }
        public Point GetFacingPoint()
        {
            Point preview = new Point(_position.X, _position.Y);

            switch (Facing)
            {
                case Direction.Up:
                    preview.Y -= 1;
                    break;
                case Direction.Down:
                    preview.Y += 1;
                    break;
                case Direction.Left:
                    preview.X -= 1;
                    break;
                case Direction.Right:
                    preview.X += 1;
                    break;
            }

            return preview;
        }

        public void Kill()
        {
            this.IsVisible = false;
        }
        public bool Hit(bool hasHelmet)
        {
            if(Health >= 0)
            {
                if (hasHelmet)
                {
                    Health -= 1;
                }
                else
                {
                    Health -= 2;
                }
            }
            if (Health < 0)
            {
                Health = 0;
            }
            if(Health == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
