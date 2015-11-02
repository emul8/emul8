//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.UserInterface.Tokenizer
{
	public abstract class Token
	{
        protected Token(string originalValue)
        {
            OriginalValue = originalValue;
        }

        public abstract object GetObjectValue();

        public string OriginalValue { get; protected set; }
	}
}
