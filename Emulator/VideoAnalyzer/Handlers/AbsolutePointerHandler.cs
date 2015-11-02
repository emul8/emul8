//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Input;
using Xwt;
using Emul8.Backends.Display.XInput;

namespace Emul8.Extensions.Analyzers.Video.Handlers
{
    internal class AbsolutePointerHandler : PointerHandler
    {
        public AbsolutePointerHandler(IAbsolutePositionPointerInput tablet, FrameBufferDisplayWidget widget) : base(tablet)
        {
            this.widget = widget;
        }

        public override void ButtonReleased(int button)
        {
            base.ButtonReleased(button);
            XLibHelper.MakeCursorTransparent(widget.ParentWindow.Id);

            //TODO: After releasing button, the real cursor over canvas is getting visible.
            // As a temporary dirty solution, variable below is set, and cursor is hidden 
            // again by moving it in MoveTablet()
            isTabletButtonJustReleased = true;
        }

        public override void PointerMoved(int x, int y, int dx, int dy)
        {
            var image = widget.Image;
            if(image == null)
            {
                return;
            }

            var ainput = (IAbsolutePositionPointerInput)input;
            var imgRect = widget.ActualImageArea;

            // if cursor doesn't touch actual image, return
            if(!imgRect.IntersectsWith(new Rectangle(x, y, 1, 1)))
            {
                XLibHelper.RestoreCursor(widget.ParentWindow.Id);
                isCursorOverImageRectangle = false;
                return;
            }
            if(!isCursorOverImageRectangle || isTabletButtonJustReleased)
            {
                widget.CanGetFocus = true;
                widget.SetFocus();
                isCursorOverImageRectangle = true;
                isTabletButtonJustReleased = false;
                XLibHelper.MakeCursorTransparent(widget.ParentWindow.Id);
            }

            //  this fragment converts click-point coordinates:
            //  from
            //     FrameBufferAnalyzer coordinates system (i.e., XWT coordinates of canvas on which the buffer was drawn)
            //  through
            //     Buffer coordinates system (i.e., resolution set on emulated graphic card)
            //  to
            //     Touchscreen coordinates system
            var maxX = (int)image.Width;
            var maxY = (int)image.Height;

            var imageX = ((x - imgRect.X) / imgRect.Width) * maxX;
            var imageY = ((y - imgRect.Y) / imgRect.Height) * maxY;

            X = (int)imageX;
            Y = (int)imageY;

            var touchscreenX = (int)Math.Round(imageX * ainput.MaxX / maxX);
            var touchscreenY = (int)Math.Round(imageY * ainput.MaxY / maxY);

            ainput.MoveTo(touchscreenX,touchscreenY);

            var opm = OnPointerMoved;
            if(opm != null)
            {
                opm(X, Y);
            }
        }

        public int X { get; protected set; }
        public int Y { get; protected set; }

        public event Action<int, int> OnPointerMoved;

        private readonly FrameBufferDisplayWidget widget;

        private bool isTabletButtonJustReleased;
        private bool isCursorOverImageRectangle;
    }
}

