using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Localization.CodeReader
{
	/// ----------------------------------------------------------------------------------------
	internal class ILInstruction
	{
		public readonly OpCode opCode;
		public readonly object operand;

		/// ------------------------------------------------------------------------------------
		public ILInstruction(OpCode opCode, object operand)
		{
			this.opCode = opCode;
			this.operand = operand;
		}
	}

	/// ----------------------------------------------------------------------------------------
	internal class ILReader : IEnumerable<ILInstruction>
	{
		private readonly Byte[] _byteArray;
		private readonly MethodBase _enclosingMethod;
		private Int32 _position;

		private static readonly OpCode[] s_OneByteOpCodes = new OpCode[0x100];
		private static readonly OpCode[] s_TwoByteOpCodes = new OpCode[0x100];

		/// ------------------------------------------------------------------------------------
		static ILReader()
		{
			foreach (var fi in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				OpCode opCode = (OpCode)fi.GetValue(null);
				UInt16 value = (UInt16)opCode.Value;

				if (value < 0x100)
					s_OneByteOpCodes[value] = opCode;
				else if ((value & 0xff00) == 0xfe00)
					s_TwoByteOpCodes[value & 0xff] = opCode;
			}
		}

		/// ------------------------------------------------------------------------------------
		public ILReader(MethodBase enclosingMethod)
		{
			_enclosingMethod = enclosingMethod;
			var methodBody = _enclosingMethod.GetMethodBody();
			_byteArray = (methodBody == null) ? new Byte[0] : methodBody.GetILAsByteArray();
			_position = 0;
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerator<ILInstruction> GetEnumerator()
		{
			while (_position < _byteArray.Length)
				yield return Next();

			_position = 0;
			yield break;
		}

		/// ------------------------------------------------------------------------------------
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// ------------------------------------------------------------------------------------
		ILInstruction Next()
		{
			//Int32 offset = _position;
			var opCode = OpCodes.Nop;

			// read first 1 or 2 bytes as opCode
			var code = ReadByte();
			if (code != 0xFE)
				opCode = s_OneByteOpCodes[code];
			else
			{
				code = ReadByte();
				opCode = s_TwoByteOpCodes[code];
			}

			object operand = null;

			switch (opCode.OperandType)
			{
				case OperandType.InlineNone: operand = null; break;
				case OperandType.ShortInlineBrTarget: operand = ReadSByte(); break;
				case OperandType.InlineBrTarget: operand = ReadInt32(); break;
				case OperandType.ShortInlineI: operand = ReadByte(); break;
				case OperandType.InlineI: operand = ReadInt32(); break;
				case OperandType.InlineI8: operand = ReadInt64(); break;
				case OperandType.ShortInlineR: operand = ReadSingle(); break;
				case OperandType.InlineR: operand = ReadDouble(); break;
				case OperandType.ShortInlineVar: operand = ReadByte(); break;
				case OperandType.InlineVar: operand = ReadUInt16(); break;
				case OperandType.InlineString: operand = ReadInt32(); break;
				case OperandType.InlineSig: operand = ReadInt32(); break;
				case OperandType.InlineField: operand = ReadInt32(); break;
				case OperandType.InlineType: operand = ReadInt32(); break;
				case OperandType.InlineTok: operand = ReadInt32(); break;
				case OperandType.InlineMethod: operand = ReadInt32(); break;
				case OperandType.InlineSwitch:
					Int32 cases = ReadInt32();
					Int32[] deltas = new Int32[cases];
					for (Int32 i = 0; i < cases; i++)
						deltas[i] = ReadInt32();
					operand = deltas;
					break;
				default:
					throw new BadImageFormatException("unexpected OperandType " + opCode.OperandType);
			}

			return new ILInstruction(opCode, operand);
		}

		Byte ReadByte() { return _byteArray[_position++]; }
		SByte ReadSByte() { return (SByte)ReadByte(); }

		UInt16 ReadUInt16() { _position += 2; return BitConverter.ToUInt16(_byteArray, _position - 2); }
		//UInt32 ReadUInt32() { _position += 4; return BitConverter.ToUInt32(_byteArray, _position - 4); }
		//UInt64 ReadUInt64() { _position += 8; return BitConverter.ToUInt64(_byteArray, _position - 8); }

		Int32 ReadInt32() { _position += 4; return BitConverter.ToInt32(_byteArray, _position - 4); }
		Int64 ReadInt64() { _position += 8; return BitConverter.ToInt64(_byteArray, _position - 8); }

		Single ReadSingle() { _position += 4; return BitConverter.ToSingle(_byteArray, _position - 4); }
		Double ReadDouble() { _position += 8; return BitConverter.ToDouble(_byteArray, _position - 8); }
	}
}
