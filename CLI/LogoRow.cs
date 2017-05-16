//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.UserInterface;
using TermSharp;
using TermSharp.Misc;
using TermSharp.Rows;
using Xwt;
using Xwt.Drawing;

namespace Emul8.CLI
{
    public class LogoRow : MonospaceTextRow
    {
        public LogoRow() : base("")
        {
            image = Image.FromResource("logo.png");
        }

        public override double PrepareForDrawing(ILayoutParameters parameters)
        {
            var baseResult = base.PrepareForDrawing(parameters);
            if(LineHeight == 0) // UI has not been initalized yet
            {
                return baseResult;
            }
            imageHeightInLines = (int)Math.Ceiling(image.Height / LineHeight);
            ceiledImageHeight = imageHeightInLines * LineHeight;
            ShellProvider.NumberOfDummyLines = imageHeightInLines;
            return baseResult;
        }

        public override void Draw(Context ctx, Rectangle selectedArea, SelectionDirection selectionDirection, TermSharp.SelectionMode selectionMode)
        {
            ctx.DrawImage(image, new Point());
            ctx.Translate(0, -ceiledImageHeight);
            base.Draw(ctx, selectedArea, selectionDirection, selectionMode);
        }

        private int imageHeightInLines;
        private double ceiledImageHeight;
        private readonly Image image;
    }
}
