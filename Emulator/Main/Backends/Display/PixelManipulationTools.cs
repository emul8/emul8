//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Emul8.Backends.Display;
using System.Linq.Expressions;
using System.Collections.Generic;
using ELFSharp.ELF;

namespace Emul8.Backends.Display
{
    public static class PixelManipulationTools
    {
        public static IPixelConverter GetConverter(PixelFormat input, Endianess inputEndianess, PixelFormat output, Endianess outputEndianess)
        {
            var tuple = Tuple.Create(input, inputEndianess, output, outputEndianess);
            if(!convertersCache.ContainsKey(tuple))
            {
                convertersCache[tuple] = ((input == output) && (inputEndianess == outputEndianess)) ? 
                    (IPixelConverter) new IdentityPixelConverter(input) : 
                    new PixelConverter(input, output, GenerateConvertMethod(input, inputEndianess, output, outputEndianess));
            }
            return convertersCache[tuple];
        }

        public static IPixelBlender GetBlender(PixelFormat backBuffer, Endianess backBufferEndianess, PixelFormat fronBuffer, Endianess frontBufferEndianes, PixelFormat output, Endianess outputEndianess)
        {
            var tuple = Tuple.Create(backBuffer, backBufferEndianess, fronBuffer, frontBufferEndianes, output, outputEndianess);
            if(!blendersCache.ContainsKey(tuple))
            {
                blendersCache[tuple] = new PixelBlender(backBuffer, fronBuffer, output, GenerateBlendMethod(backBuffer, backBufferEndianess, fronBuffer, frontBufferEndianes, output, outputEndianess));
            }
            return blendersCache[tuple];
        }

        private static BlendDelegate GenerateBlendMethod(PixelFormat backBuffer, Endianess backBufferEndianess, PixelFormat frontBuffer, Endianess frontBufferEndianess, PixelFormat output, Endianess outputEndianess)
        {
            var vRed = Expression.Variable(typeof(uint),    "red");
            var vGreen = Expression.Variable(typeof(uint),  "green");
            var vBlue = Expression.Variable(typeof(uint),   "blue");
            var vAlpha = Expression.Variable(typeof(uint),  "alpha");

            var vBackRed = Expression.Variable(typeof(uint),    "backRed");
            var vBackGreen = Expression.Variable(typeof(uint),  "backGreen");
            var vBackBlue = Expression.Variable(typeof(uint),   "backBlue");
            var vBackAlpha = Expression.Variable(typeof(uint),  "backAlpha");

            var vFrontRed = Expression.Variable(typeof(uint),    "frontRed");
            var vFrontGreen = Expression.Variable(typeof(uint),  "frontGreen");
            var vFrontBlue = Expression.Variable(typeof(uint),   "frontBlue");
            var vFrontAlpha = Expression.Variable(typeof(uint),  "frontAlpha");

            var vBackgroundColorRed = Expression.Variable(typeof(uint), "backgroundColorRed");
            var vBackgroundColorGreen = Expression.Variable(typeof(uint), "backgroundColorGreen");
            var vBackgroundColorBlue = Expression.Variable(typeof(uint), "backgroundColorBlue");
            var vBackgroundColorAlpha = Expression.Variable(typeof(uint), "backgroundColorAlpha");

            var vBackPos = Expression.Variable(typeof(int), "backPos");
            var vFrontPos = Expression.Variable(typeof(int), "frontPos");
            var vOutPos = Expression.Variable(typeof(int), "outPos");
            var vBackStep = Expression.Variable(typeof(int), "backStep");
            var vFrontStep = Expression.Variable(typeof(int), "frontStep");
            var vOutStep = Expression.Variable(typeof(int), "outStep");
            var vLength = Expression.Variable(typeof(int),  "length");

            var vBackBuffer = Expression.Parameter(typeof(byte[]), "backBuffer");
            var vFrontBuffer = Expression.Parameter(typeof(byte[]), "fronBuffer");
            var vOutputBuffer = Expression.Parameter(typeof(byte[]).MakeByRefType(), "outputBuffer");
            var vBackgroundColor = Expression.Parameter(typeof(Pixel), "backgroundColor");
            var vBackBufferAlphaMultiplier = Expression.Parameter(typeof(byte), "backBufferAlphaMultiplier");
            var vFrontBufferAlphaMultiplier = Expression.Parameter(typeof(byte), "frontBufferAlphaMultiplier");

            var vBackAlphaBlended = Expression.Variable(typeof(uint), "backAlphaBlended");
            var vBackgroundColorAlphaBlended = Expression.Variable(typeof(uint), "backgroundColorAlphaBlended");

            var outOfLoop = Expression.Label();

            var block = Expression.Block(
                new [] { vRed, vGreen, vBlue, vAlpha,
                         vFrontRed, vFrontGreen, vFrontBlue, vFrontAlpha,
                         vBackRed, vBackGreen, vBackBlue, vBackAlpha,
                         vBackStep, vFrontStep, vOutStep, vLength, vBackPos, vFrontPos, vOutPos,
                         vBackgroundColorRed, vBackgroundColorGreen, vBackgroundColorBlue, vBackgroundColorAlpha, 
                         vBackAlphaBlended, vBackgroundColorAlphaBlended
                },

                Expression.Assign(vBackStep, Expression.Constant(backBuffer.GetColorDepth())),
                Expression.Assign(vFrontStep, Expression.Constant(frontBuffer.GetColorDepth())),
                Expression.Assign(vOutStep, Expression.Constant(output.GetColorDepth())),
                Expression.Assign(vLength, Expression.Property(vBackBuffer, "Length")),

                Expression.Assign(vBackPos, Expression.Constant(0x00)),
                Expression.Assign(vFrontPos, Expression.Constant(0x00)),
                Expression.Assign(vOutPos, Expression.Constant(0x00)),

                Expression.Assign(vBackgroundColorAlpha, Expression.Convert(Expression.Property(vBackgroundColor, "Alpha"), typeof(uint))),
                Expression.Assign(vBackgroundColorRed, Expression.Convert(Expression.Property(vBackgroundColor, "Red"), typeof(uint))),
                Expression.Assign(vBackgroundColorGreen, Expression.Convert(Expression.Property(vBackgroundColor, "Green"), typeof(uint))),
                Expression.Assign(vBackgroundColorBlue, Expression.Convert(Expression.Property(vBackgroundColor, "Blue"), typeof(uint))),

                Expression.Loop(
                    Expression.IfThenElse(Expression.LessThan(vBackPos, vLength),
                        Expression.Block(

                            GenerateFrom(backBuffer, backBufferEndianess, vBackBuffer, vBackPos, vBackRed, vBackGreen, vBackBlue, vBackAlpha),
                            GenerateFrom(frontBuffer, frontBufferEndianess, vFrontBuffer, vFrontPos, vFrontRed, vFrontGreen, vFrontBlue, vFrontAlpha),

                            Expression.Assign(vBackAlpha, Expression.Divide(Expression.Multiply(vBackAlpha, Expression.Convert(vBackBufferAlphaMultiplier, typeof(uint))), Expression.Constant((uint)0xFF))),
                            Expression.Assign(vFrontAlpha, Expression.Divide(Expression.Multiply(vFrontAlpha, Expression.Convert(vFrontBufferAlphaMultiplier, typeof(uint))), Expression.Constant((uint)0xFF))),

                            Expression.Block(
                                // (b_alpha * (0xFF - f_alpha)) / 0xFF
                                Expression.Assign(
                                    vBackAlphaBlended,
                                    Expression.Divide(
                                        Expression.Multiply(
                                            vBackAlpha,
                                            Expression.Subtract(
                                                Expression.Constant((uint)0xFF),
                                                vFrontAlpha)),
                                        Expression.Constant((uint)0xFF))),

                                // (c_alpha * (0xFF - (f_alpha + b_alpha * (0xFF - f_alpha)))) / 0xFF
                                Expression.Assign(
                                        vBackgroundColorAlphaBlended,
                                        Expression.Divide(
                                            Expression.Multiply(
                                                vBackgroundColorAlpha,
                                                Expression.Subtract(
                                                    Expression.Constant((uint)0xFF),
                                                    Expression.Add(
                                                        vFrontAlpha,
                                                        vBackAlphaBlended))),
                                             Expression.Constant((uint)0xFF))),


                                Expression.Assign(
                                    vAlpha, 
                                    Expression.Add(
                                        vFrontAlpha,
                                        Expression.Add(
                                            vBackAlphaBlended,
                                            vBackgroundColorAlphaBlended))),

                                Expression.IfThenElse(
                                    Expression.Equal(vAlpha, Expression.Constant((uint)0)),
                                    Expression.Block(
                                        Expression.Assign(vRed, vBackgroundColorRed),
                                        Expression.Assign(vGreen, vBackgroundColorGreen),
                                        Expression.Assign(vBlue, vBackgroundColorBlue),
                                        Expression.Assign(vAlpha, vBackgroundColorAlpha)),
                                    Expression.Block( 
                                        Expression.Assign(
                                            vRed, 
                                            Expression.Divide(
                                                Expression.Add(
                                                    Expression.Add(
                                                        Expression.Multiply(vBackgroundColorAlphaBlended, vBackgroundColorRed),
                                                        Expression.Multiply(vBackAlphaBlended, vBackRed)),
                                                    Expression.Multiply(vFrontAlpha, vFrontRed)),
                                                vAlpha)),
                                        Expression.Assign(
                                            vGreen, 
                                            Expression.Divide(
                                                Expression.Add(
                                                    Expression.Add(
                                                        Expression.Multiply(vBackgroundColorAlphaBlended, vBackgroundColorGreen),
                                                        Expression.Multiply(vBackAlphaBlended, vBackGreen)),
                                                    Expression.Multiply(vFrontAlpha, vFrontGreen)),
                                                vAlpha)),
                                        Expression.Assign(
                                            vBlue, 
                                            Expression.Divide(
                                                Expression.Add(
                                                    Expression.Add(
                                                        Expression.Multiply(vBackgroundColorAlphaBlended, vBackgroundColorBlue),
                                                        Expression.Multiply(vBackAlphaBlended, vBackBlue)),
                                                    Expression.Multiply(vFrontAlpha, vFrontBlue)),
                                                vAlpha))))
                            ),

                            GenerateTo(output, outputEndianess, vOutputBuffer, vOutPos, vRed, vGreen, vBlue, vAlpha),

                            Expression.AddAssign(vBackPos, vBackStep),
                            Expression.AddAssign(vFrontPos, vFrontStep),
                            Expression.AddAssign(vOutPos, vOutStep)
                        ),
                        Expression.Break(outOfLoop)
                    ),
                    outOfLoop
                )
            );

            return Expression.Lambda<BlendDelegate>(block, vBackBuffer, vFrontBuffer, vOutputBuffer, vBackgroundColor, vBackBufferAlphaMultiplier, vFrontBufferAlphaMultiplier).Compile();
        }

        private static ConvertDelegate GenerateConvertMethod(PixelFormat input, Endianess inputEndianess, PixelFormat output, Endianess outputEndianess)
        {
            var vRed = Expression.Variable(typeof(uint),    "red");
            var vGreen = Expression.Variable(typeof(uint),  "green");
            var vBlue = Expression.Variable(typeof(uint),   "blue");
            var vAlpha = Expression.Variable(typeof(uint),  "alpha");

            var vInStep = Expression.Variable(typeof(int),  "inStep");
            var vOutStep = Expression.Variable(typeof(int), "outStep");
            var vLength = Expression.Variable(typeof(int),  "length");

            var vInputBuffer = Expression.Parameter(typeof(byte[]), "inputBuffer");
            var vOutputBuffer = Expression.Parameter(typeof(byte[]).MakeByRefType(), "outputBuffer");

            var vInPos = Expression.Variable(typeof(int), "inPos");
            var vOutPos = Expression.Variable(typeof(int), "outPos");

            var outOfLoop = Expression.Label();

            var block = Expression.Block(
                new [] { vRed, vGreen, vBlue, vAlpha, vInStep, vOutStep, vLength, vInPos, vOutPos },

                Expression.Assign(vInStep, Expression.Constant(input.GetColorDepth())),
                Expression.Assign(vOutStep, Expression.Constant(output.GetColorDepth())),
                Expression.Assign(vLength, Expression.Property(vInputBuffer, "Length")),

                Expression.Assign(vInPos, Expression.Constant(0)),
                Expression.Assign(vOutPos, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(Expression.LessThan(vInPos, vLength),
                        Expression.Block(
                            GenerateFrom(input, inputEndianess, vInputBuffer, vInPos, vRed, vGreen, vBlue, vAlpha),
                            GenerateTo(output, outputEndianess, vOutputBuffer, vOutPos, vRed, vGreen, vBlue, vAlpha),

                            Expression.AddAssign(vInPos, vInStep),
                            Expression.AddAssign(vOutPos, vOutStep)
                        ),
                        Expression.Break(outOfLoop)
                    ),
                    outOfLoop
                )
            );

            return Expression.Lambda<ConvertDelegate>(block, vInputBuffer, vOutputBuffer).Compile();
        }

        /// <summary>
        /// Generates expression reading one pixel encoded in given format from input buffer (with bytes ordered accordingly to endianess) at given position 
        /// and storing each channel in separate variable expression.
        /// </summary>
        /// <returns>Generated expression.</returns>
        /// <param name="inputFormat">Pixel color format of pixels in input buffer.</param>
        /// <param name="endianess">Endianess of input buffer.</param>
        /// <param name="inBuffer">Input buffer.</param>
        /// <param name="inPosition">Position of pixel in buffer.</param>
        /// <param name="redByte">Variable to which value of red channel should be stored.</param>
        /// <param name="greenByte">Variable to which value of green channel should be stored.</param>
        /// <param name="blueByte">Variable to which value of blue channel should be stored.</param>
        /// <param name="alphaByte">Variable to which value of alpha channel should be stored.</param>
        private static Expression GenerateFrom(PixelFormat inputFormat, Endianess endianess, Expression inBuffer, Expression inPosition, Expression redByte, Expression greenByte, Expression blueByte, Expression alphaByte)
        {
            byte currentBit = 0;
            byte currentByte = 0;
            bool isAlphaSet = false;
            
            var expressions = new List<Expression>();
            var inputBytes = new ParameterExpression[inputFormat.GetColorDepth()];
            for(int i = 0; i < inputBytes.Length; i++)
            {
                inputBytes[i] = Expression.Variable(typeof(uint));
                
                expressions.Add(
                    Expression.Assign(
                        inputBytes[i], 
                        Expression.Convert(
                            Expression.ArrayIndex(
                                inBuffer,
                                Expression.Add(
                                    inPosition, 
                                    Expression.Constant((endianess == Endianess.BigEndian) ? i : inputBytes.Length - i - 1))),
                            typeof(uint))));
            }

            foreach(var colorDescriptor in inputFormat.GetColorsLengths())
            {
                Expression colorExpression = null; 
                
                foreach(var transformation in ByteInflate(colorDescriptor.Value, currentBit))
                {
                    Expression currentExpressionFragment = inputBytes[currentByte];
                    
                    if(transformation.MaskBits != 0xFF)
                    {
                        currentExpressionFragment = Expression.And(currentExpressionFragment, Expression.Constant((uint)transformation.MaskBits));
                    }
                    
                    if(transformation.ShiftBits > 0)
                    {
                        currentExpressionFragment = Expression.RightShift(currentExpressionFragment, Expression.Constant((int)transformation.ShiftBits));
                    }
                    else if(transformation.ShiftBits < 0)
                    {
                        currentExpressionFragment = Expression.And(
                            Expression.LeftShift(currentExpressionFragment, Expression.Constant((int)(-transformation.ShiftBits))),
                            Expression.Constant((uint)0xFF));
                    }
                    
                    currentBit += transformation.UsedBits;
                    if(currentBit >= 8)
                    {
                        currentBit -= 8;
                        currentByte++;
                    }
                    
                    colorExpression = (colorExpression == null) ? currentExpressionFragment : Expression.Or(colorExpression, currentExpressionFragment);
                }

                if(colorDescriptor.Key == ColorType.X)
                {
                    continue;
                }

                Expression currentColor = null;
                switch(colorDescriptor.Key)
                {
                case ColorType.A:
                    currentColor = alphaByte;
                    isAlphaSet = true;
                    break;
                case ColorType.B:
                    currentColor = blueByte;
                    break;
                case ColorType.G:
                    currentColor = greenByte;
                    break;
                case ColorType.R:
                    currentColor = redByte;
                    break;
                }
                
                expressions.Add(Expression.Assign(currentColor, colorExpression));
                
                // filling lsb '0'-bits with copy of msb pattern
                var numberOfBits = colorDescriptor.Value;
                var zeroBits = 8 - numberOfBits;
                while(zeroBits > 0)
                {
                    expressions.Add(Expression.OrAssign(
                        currentColor, 
                        Expression.RightShift(
                            currentColor, 
                            Expression.Constant((int)numberOfBits))));
                    zeroBits -= numberOfBits;
                }
            }

            if(!isAlphaSet)
            {
                expressions.Add(Expression.Assign(alphaByte, Expression.Constant((uint)0xFF)));
            }
            return Expression.Block(inputBytes, expressions);
        }

        /// <summary>
        /// Generates expression converting and storing one pixel in output format to output buffer (ordering bytes accordingly to endianess) at given position using channels values provided by red/green/blue/alpha variables.
        /// </summary>
        /// <returns>Generated expression.</returns>
        /// <param name="outputFormat">Pixel color format of pixels stored in output buffer.</param>
        /// <param name="endianess">Endianess of output buffer.</param>
        /// <param name="outBuffer">Output buffer.</param>
        /// <param name="outPosition">Position of pixel in output buffer.</param>
        /// <param name="redByte">Variable from which value of red channel should be read.</param>
        /// <param name="greenByte">Variable from which value of green channel should be read.</param>
        /// <param name="blueByte">Variable from which value of blue channel should be read.</param>
        /// <param name="alphaByte">Variable from which value of alpha channel should be read.</param>
        private static Expression GenerateTo(PixelFormat outputFormat, Endianess endianess, Expression outBuffer, Expression outPosition, Expression redByte, Expression greenByte, Expression blueByte, Expression alphaByte)
        {
            byte currentBit = 0;
            byte currentByte = 0;
            var expressions = new List<Expression>();
            
            Expression currentExpression = null;
            
            foreach(var colorDescriptor in outputFormat.GetColorsLengths())
            {
                Expression colorExpression = null; 
                
                switch(colorDescriptor.Key)
                {
                case ColorType.A:
                    colorExpression = alphaByte;
                    break;
                case ColorType.B:
                    colorExpression = blueByte;
                    break;
                case ColorType.G:
                    colorExpression = greenByte;
                    break;
                case ColorType.R:
                    colorExpression = redByte;
                    break;
                case ColorType.X:
                    colorExpression = Expression.Constant((uint)0xFF);
                    break;
                }

                foreach(var transformation in ByteSqueezeAndMove(colorDescriptor.Value, currentBit))
                {
                    Expression currentExpressionFragment = colorExpression;
                
                    if(transformation.MaskBits != 0xFF)
                    {
                        currentExpressionFragment = Expression.And(currentExpressionFragment, Expression.Constant((uint)transformation.MaskBits));
                    }

                    if(transformation.ShiftBits > 0)
                    {
                        currentExpressionFragment = Expression.RightShift(currentExpressionFragment, Expression.Constant((int)transformation.ShiftBits));
                    }
                    else if(transformation.ShiftBits < 0)
                    {
                        currentExpressionFragment = Expression.And(
                            Expression.LeftShift(currentExpressionFragment, Expression.Constant((int)(-transformation.ShiftBits))),
                            Expression.Constant((uint)0xFF));
                                
                    }
                    
                    currentExpression = (currentExpression == null) ? currentExpressionFragment : Expression.Or(currentExpression, currentExpressionFragment);

                    currentBit += transformation.UsedBits;
                    while(currentBit >= 8)
                    {
                        expressions.Add(
                            Expression.Assign(
                                Expression.ArrayAccess(outBuffer, 
                                    Expression.Add(
                                        outPosition, 
                                        Expression.Constant((endianess == Endianess.BigEndian) ? (int) currentByte : (outputFormat.GetColorDepth() - currentByte - 1)))),
                                Expression.Convert(currentExpression, typeof(byte))));

                        currentExpression = null;
                        currentBit -= 8;
                        currentByte++;
                    }
                }
            }

            return Expression.Block(expressions);
        }

        /// <summary>
        /// Calculates a set of transformations that reduces a byte variable into lower number of bits (by cutting off the least significant bits)
        /// and shifts them by given offset.
        /// </summary>
        /// <returns>Set of byte transformations.</returns>
        /// <param name="bits">Number of bits of resulting data.</param>
        /// <param name="offset">Number of bits by which resulting data should be shifted (in right direction).</param>
        private static TransformationDescriptor[] ByteSqueezeAndMove(byte bits, byte offset)
        {
            if(offset < 0 || offset > 7)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if(bits == 0 || bits > 8)
            {
                throw new ArgumentOutOfRangeException("bits");
            }

            var result = new List<TransformationDescriptor>();

            var bitsLeft = bits;
            var additionalShift = 0;
            while(bitsLeft > 0)
            {
                var currentBits = (byte)Math.Min(8 - offset, bitsLeft);
                var mask = (byte)(((1 << currentBits) - 1) << (8 - additionalShift - currentBits));
                result.Add(new TransformationDescriptor((sbyte)(bitsLeft < bits ? -additionalShift : offset), mask, currentBits));

                additionalShift += currentBits;
                bitsLeft -= currentBits;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Calculates a set of transformations that reads reduced byte variable of size 'bits' shifted by 'bitOffset'.
        /// </summary>
        /// <returns>Set of byte transformations.</returns>
        /// <param name="bits">Number of bits of input data.</param>
        /// <param name="bitOffset">Number of bits by which input data is shifted.</param>
        private static TransformationDescriptor[] ByteInflate(byte bits, byte bitOffset)
        {
            if(bits == 0 || bits > 8)
            {
                throw new ArgumentOutOfRangeException("bits");
            }

            if(bitOffset > 7)
            {
                throw new ArgumentOutOfRangeException("bitOffset");
            }

            var result = new List<TransformationDescriptor>();

            var bitsLeft = bits;
            var additionalShift = 0;

            while(bitsLeft > 0)
            {
                var currentBits = (byte)Math.Min(8 - bitOffset, bitsLeft);
                var mask = ((1 << currentBits) - 1) << (8 - bitOffset - currentBits);
                var shift = (sbyte)(-bitOffset + additionalShift);

                // optimization
                if ((mask >> shift) == (0xFF >> shift))
                {
                    mask = 0xFF;
                }

                var descriptor = new TransformationDescriptor(shift, (byte)mask, currentBits);
                result.Add(descriptor);

                bitOffset = 0;
                additionalShift += currentBits;
                bitsLeft -= currentBits;
            }

            return result.ToArray();
        }
        
        private struct TransformationDescriptor
        {
            public TransformationDescriptor(sbyte shift, byte mask, byte usedBits) : this()
            {
                ShiftBits = shift;
                MaskBits = mask;
                UsedBits = usedBits;
            }

            public sbyte ShiftBits { get; private set; }
            public byte  MaskBits  { get; private set; }
            public byte  UsedBits  { get; private set; }
        }
        
        private class PixelConverter : IPixelConverter
        {
            public PixelConverter(PixelFormat input, PixelFormat output, ConvertDelegate converter)
            {
                Input = input;
                Output = output;
                this.converter = converter;
            }

            public void Convert(byte[] inBuffer, ref byte[] outBuffer)
            {
                converter(inBuffer, ref outBuffer);
            }

            public PixelFormat Input { get; private set; }
            public PixelFormat Output { get; private set; }
            
            private readonly ConvertDelegate converter;
        }

        private class IdentityPixelConverter : IPixelConverter
        {
            public IdentityPixelConverter(PixelFormat format)
            {
                Input = format;
                Output = format;
            }

            public void Convert(byte[] inBuffer, ref byte[] outBuffer)
            {
                outBuffer = inBuffer;
            }

            public PixelFormat Input { get; private set; }
            public PixelFormat Output { get; private set; }
        }

        private class PixelBlender : IPixelBlender
        {
            public PixelBlender(PixelFormat back, PixelFormat front, PixelFormat output, BlendDelegate blender)
            {
                BackBuffer = back;
                FrontBuffer = front;
                Output = output;
                this.blender = blender;
            }

            public void Blend(byte[] backBuffer, byte[] frontBuffer, ref byte[] output, Pixel background = null, byte backBufferAlphaMulitplier = 0xFF, byte frontBufferAlphaMultiplayer = 0xFF)
            {
                if(background == null)
                {
                    background = new Pixel(0x00, 0x00, 0x00, 0x00);
                }
                blender(backBuffer, frontBuffer, ref output, background, backBufferAlphaMulitplier, frontBufferAlphaMultiplayer);
            }

            public PixelFormat BackBuffer { get; private set; }
            public PixelFormat FrontBuffer { get; private set; }
            public PixelFormat Output { get; private set; } 

            private readonly BlendDelegate blender;
        }

        private static Dictionary<Tuple<PixelFormat, Endianess, PixelFormat, Endianess>, IPixelConverter> convertersCache = new Dictionary<Tuple<PixelFormat, Endianess, PixelFormat, Endianess>, IPixelConverter>();
        private static Dictionary<Tuple<PixelFormat, Endianess, PixelFormat, Endianess, PixelFormat, Endianess>, IPixelBlender> blendersCache = new Dictionary<Tuple<PixelFormat, Endianess, PixelFormat, Endianess, PixelFormat, Endianess>, IPixelBlender>();

        private delegate void ConvertDelegate(byte[] inBuffer, ref byte[] outBuffer);
        private delegate void BlendDelegate(byte[] backBuffer, byte[] frontBuffer, ref byte[] outBuffer, Pixel background = null, byte backBufferAlphaMulitplier = 0xFF, byte frontBufferAlphaMultiplayer = 0xFF);
    }
}
