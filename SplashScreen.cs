﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.DrawCalls;
using SadConsole.Effects;
using SadConsole.Instructions;
using Console = SadConsole.Console;

namespace RunicQuest
{
    internal class SplashScreen : ScrollingConsole
    {
        public Action SplashCompleted { get; set; }

        private readonly Console _consoleImage;
        private readonly Point _consoleImagePosition;
        private int _gradientPositionX = -50;

        public SplashScreen()
            : base(80, 25)
        {
            Cursor.IsEnabled = false;
            IsVisible = false;

            // Setup the console text background pattern, which is hidden via colors
            // Print the text template on all of the console surface
            const string textTemplate = "Runic Quest";
            var text = new System.Text.StringBuilder(Width * Height);

            for (int i = 0; i < Width * Height; i++)
            {
                text.Append(textTemplate);
            }
            Print(0, 0, text.ToString(), Color.Black, Color.Transparent);

            // Load the logo and convert to a console
            using (System.IO.Stream imageStream = Microsoft.Xna.Framework.TitleContainer.OpenStream("assets/test.png"))
            {
                using (var image = Texture2D.FromStream(Global.GraphicsDevice, imageStream))
                {
                    CellSurface logo = image.ToSurface(Global.FontDefault, false);

                    _consoleImage = Console.FromSurface(logo, Global.FontDefault);
                    _consoleImagePosition = new Point(Width / 2 - _consoleImage.Width / 2, -1);
                    _consoleImage.Tint = Color.Black;
                }
            }

            // Animation for the logo text.
            var logoText = new ColorGradient(new[] { Color.Magenta, Color.Yellow }, new[] { 0.0f, 1f })
                               .ToColoredString("[| Powered by SadConsole |]");
            logoText.SetEffect(new Fade()
            {
                DestinationForeground = Color.Blue,
                FadeForeground = true,
                FadeDuration = 1f,
                Repeat = false,
                RemoveOnFinished = true,
                Permanent = true,
                CloneOnApply = true
            });

            // Configure the animation
            InstructionSet animation = new InstructionSet()

                    .Wait(TimeSpan.FromSeconds(0.3d))

                    // Animation to move the angled gradient spotlight effect
                    .Code(MoveGradient)

                    // Clear the background text so new printing doesn't look bad
                    .Code((console, delta) =>
                    {
                        console.Fill(Color.Black, Color.Transparent, 0);
                        return true;
                    })

                    // Draw the SadConsole text at the bottom
                    .Instruct(new DrawString(logoText)
                    {
                        Position = new Point(26, Height - 1),
                        TotalTimeToPrint = 1f
                    })

                    // Fade in the logo
                    .Instruct(new FadeTextSurfaceTint(_consoleImage,
                                                      new ColorGradient(Color.Black, Color.Transparent),
                                                      TimeSpan.FromSeconds(2)))

                    // Blink SadConsole text at the bottom
                    .Code(SetBlinkOnLogoText)

                    // Delay so blinking effect is seen
                    .Wait(TimeSpan.FromSeconds(2.5d))

                    // Fade out main console and logo console.
                    .InstructConcurrent(new FadeTextSurfaceTint(_consoleImage,
                                                      new ColorGradient(Color.Transparent, Color.Black),
                                                      TimeSpan.FromSeconds(2)),

                                        new FadeTextSurfaceTint(this,
                                                      new ColorGradient(Color.Transparent, Color.Black),
                                                      TimeSpan.FromSeconds(1.0d)))

                    // Animation has completed, call the callback this console uses to indicate it's complete
                    .Code((con, delta) => { SplashCompleted?.Invoke(); return true; })
                ;

            animation.RemoveOnFinished = true;

            Components.Add(animation);
        }

        public override void Update(TimeSpan delta)
        {
            if (!IsVisible)
            {
                return;
            }

            base.Update(delta);
        }

        public override void Draw(TimeSpan delta)
        {
            // Draw the logo console...
            if (IsVisible)
            {
                Renderer.Render(_consoleImage);
                Global.DrawCalls.Add(new DrawCallScreenObject(_consoleImage, _consoleImagePosition, false));

                base.Draw(delta);
            }
        }

        private bool MoveGradient(Console console, TimeSpan delta)
        {
            _gradientPositionX += 1;

            if (_gradientPositionX > Width + 50)
            {
                return true;
            }

            Color[] colors = new[] { Color.Black, Color.Blue, Color.White, Color.Blue, Color.Black };
            float[] colorStops = new[] { 0f, 0.2f, 0.5f, 0.8f, 1f };

            Algorithms.GradientFill(Font.Size, new Point(_gradientPositionX, 12), 10, 45, new Rectangle(0, 0, Width, Height), new ColorGradient(colors, colorStops), SetForeground);

            return false;
        }

        private bool SetBlinkOnLogoText(Console console, TimeSpan delta)
        {
            var fadeEffect = new Fade
            {
                AutoReverse = true,
                DestinationForeground = new ColorGradient(Color.Blue, Color.Yellow),
                FadeForeground = true,
                UseCellForeground = false,
                Repeat = true,
                FadeDuration = 0.7f,
                RemoveOnFinished = true
            };

            var cells = new List<Cell>();
            for (int index = 0; index < 10; index++)
            {
                int point = new Point(26, Height - 1).ToIndex(Width) + 14 + index;
                cells.Add(Cells[point]);
            }

            SetEffect(cells, fadeEffect);
            return true;
        }
    }
}