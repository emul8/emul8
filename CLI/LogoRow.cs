//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using TermSharp;
using TermSharp.Misc;
using TermSharp.Rows;
using Xwt;
using Xwt.Drawing;

namespace Emul8.CLI
{
    public class LogoRow : MonospaceTextRow
    {
        public LogoRow(string content) : base(content)
        {
            image = Image.FromFile("External/emul8-libraries/Graphics/logo.png");
        }

        public override double PrepareForDrawing(ILayoutParameters parameters)
        {
            var baseResult = base.PrepareForDrawing(parameters);
            imageHeightInLines = (int)Math.Ceiling(image.Height / LineHeight);
            ceiledImageHeight = imageHeightInLines * LineHeight;
            return baseResult + ceiledImageHeight;
        }

        public override void Draw(Context ctx, Rectangle selectedArea, SelectionDirection selectionDirection, TermSharp.SelectionMode selectionMode)
        {
            ctx.DrawImage(image, new Point());
            ctx.Translate(0, ceiledImageHeight);
            base.Draw(ctx, selectedArea, selectionDirection, selectionMode);
        }

        public override void DrawCursor(Context ctx, int offset, bool focused)
        {
            ctx.Translate(0, ceiledImageHeight);
            base.DrawCursor(ctx, offset, focused);
            ctx.Translate(0, -ceiledImageHeight);
        }

        public override int SublineCount
        {
            get
            {
                return base.SublineCount + imageHeightInLines;
            }
        }

        private int imageHeightInLines;
        private double ceiledImageHeight;
        private readonly Image image;
    }
}
