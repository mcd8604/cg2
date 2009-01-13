using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace RayTracerXNA
{
    class ScreenCapture : DrawableGameComponent
    {
        private ResolveTexture2D resolveTarget;

        public ScreenCapture(Game game)
            : base(game) { }

        protected override void LoadContent()
        {
            resolveTarget = new ResolveTexture2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                GraphicsDevice.PresentationParameters.BackBufferFormat);

            base.LoadContent();
        }

        private KeyboardState lastKeyState = Keyboard.GetState();

        public override void Update(GameTime gameTime)
        {
            KeyboardState curKeyState = Keyboard.GetState();

            if (lastKeyState.IsKeyUp(Keys.PrintScreen) && curKeyState.IsKeyDown(Keys.PrintScreen))
            {
                saveTexture();
            }

            base.Update(gameTime);
        }

        private bool resolved = false;

        private void saveTexture()
        {
            if (!resolved)
            {
                int count = 0;
                while (File.Exists("Capture" + count + ".png"))
                {
                    ++count;
                }

                GraphicsDevice.ResolveBackBuffer(resolveTarget);
                resolved = true;
                resolveTarget.Save("Capture" + count + ".png", ImageFileFormat.Png);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            resolved = false;
            base.Draw(gameTime);
        }
    }
}
