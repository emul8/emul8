//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.Input;
using Xwt;

namespace Emul8.Extensions.Analyzers.Video.Handlers
{
    internal abstract class PointerHandler
    {
        protected PointerHandler(IPointerInput input)
        {
            this.input = input;
        }

        public virtual void Init()
        {
        }

        public virtual void ButtonPressed(int button)
        {
            input.Press(ToMouseButton((PointerButton)button));
        }

        public virtual void ButtonReleased(int button)
        {
            input.Release(ToMouseButton((PointerButton)button));
        }

        public abstract void PointerMoved(int x, int y, int dx, int dy);

        private MouseButton ToMouseButton(PointerButton button)
        {
            switch(button)
            {
            case PointerButton.Left:
                return MouseButton.Left;
            case PointerButton.Right:
                return MouseButton.Right;
            case PointerButton.Middle:
                return MouseButton.Middle;
            }

            return MouseButton.Extra;
        }

        protected IPointerInput input;
    }
}

