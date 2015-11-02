//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Backends.Display.XInput
{
	public interface IInputHandler
	{
        void ButtonPressed(int button);
        void ButtonReleased(int button);

        void KeyPressed(int key);
        void KeyReleased(int key);

		void MouseMoved(int x, int y, int dx, int dy);

		bool Stop { get; set; }
		bool CursorFixed { get; }
	}
}

