//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Emul8.UserInterface.Tokenizer;
using Emul8.Exceptions;

namespace Emul8.UserInterface
{

	public class TokenizationResult
	{
		public TokenizationResult(int unmatchedCharactersLeft, IEnumerable<Token> tokens, RecoverableException e)
		{
			UnmatchedCharactersLeft = unmatchedCharactersLeft;
			Tokens = tokens;
            Exception = e;
		}

		public int UnmatchedCharactersLeft { get; private set; }
		public IEnumerable<Token> Tokens { get; private set; }
        public RecoverableException Exception { get; private set; }

        public override string ToString()
        {
            return String.Join("", Tokens.Select(x => x.ToString())) + ((UnmatchedCharactersLeft != 0) ? String.Format(" (unmatched characters: {0})", UnmatchedCharactersLeft) : ""
                                                                        + Exception != null ? String.Format(" Exception message: {0}",Exception.Message):"");
        }
	}

}
