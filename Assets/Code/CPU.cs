using System;
using System.Collections.Generic;
using com.PixelismGames.UnityGameBoy.Utilities;
using UnityEngine;

namespace com.PixelismGames.UnityGameBoy
{
    [AddComponentMenu("Pixelism Games/CPU")]
    public class CPU : MonoBehaviour
    {
        [ReadOnly] public byte A;
        [ReadOnly] public byte B;
        [ReadOnly] public byte C;
        [ReadOnly] public byte D;
        [ReadOnly] public byte E;
        [ReadOnly] public byte H;
        [ReadOnly] public byte L;
        [ReadOnly] public ushort PC;
        [ReadOnly] public ushort SP;

        [ReadOnly] public bool Zf;
        [ReadOnly] public bool Cf;
        [ReadOnly] public bool Nf;
        [ReadOnly] public bool Hf;

        private Dictionary<byte, Action<byte>> _opcodeMap;
        private Dictionary<byte, Action<byte>> _opcodeExtendedMap;
        private Dictionary<byte, Action> _opcodeMap4X;
        private Dictionary<byte, Action> _opcodeMap5X;
        private Dictionary<byte, Action> _opcodeMap6X;
        private Dictionary<byte, Action> _opcodeMap7X;
        private Dictionary<byte, Action> _opcodeMapAX;
        private Dictionary<byte, Action> _opcodeMapBX;

        #region Properties

        public ushort BC
        {
            get { return ((ushort)((B << 8) | C)); }
            set
            {
                B = (byte)((value & 0xFF00) >> 8);
                C = (byte)(value & 0x00FF);
            }
        }

        public ushort DE
        {
            get { return ((ushort)((D << 8) | E)); }
            set
            {
                D = (byte)((value & 0xFF00) >> 8);
                E = (byte)(value & 0x00FF);
            }
        }

        public ushort HL
        {
            get { return ((ushort)((H << 8) | L)); }
            set
            {
                H = (byte)((value & 0xFF00) >> 8);
                L = (byte)(value & 0x00FF);
            }
        }

        #endregion

        #region MonoBehaviour

        public void Awake()
        {
            _opcodeMap = new Dictionary<byte, Action<byte>>();
            _opcodeMap.Add(0x00, noOperation);
            _opcodeMap.Add(0x01, (x) => BC = loadImmediateWord());
            _opcodeMap.Add(0x04, (x) => B = increment(B));
            _opcodeMap.Add(0x05, (x) => B = decrement(B));
            _opcodeMap.Add(0x06, (x) => B = loadImmediate());
            _opcodeMap.Add(0x07, rotateALeft);
            _opcodeMap.Add(0x08, (x) => Debug.Log("Write Word to Immediate Address (not implemented)"));
            _opcodeMap.Add(0x09, (x) => HL = add(HL, BC));
            _opcodeMap.Add(0x0C, (x) => C = increment(C));
            _opcodeMap.Add(0x0D, (x) => C = decrement(C));
            _opcodeMap.Add(0x0E, (x) => C = loadImmediate());
            _opcodeMap.Add(0x11, (x) => DE = loadImmediateWord());
            _opcodeMap.Add(0x14, (x) => D = increment(D));
            _opcodeMap.Add(0x15, (x) => D = decrement(D));
            _opcodeMap.Add(0x16, (x) => D = loadImmediate());
            _opcodeMap.Add(0x18, jumpRelative);
            _opcodeMap.Add(0x19, (x) => HL = add(HL, DE));
            _opcodeMap.Add(0x1C, (x) => E = increment(E));
            _opcodeMap.Add(0x1D, (x) => E = decrement(E));
            _opcodeMap.Add(0x1E, (x) => E = loadImmediate());
            _opcodeMap.Add(0x1F, rotateARightThroughCarry);
            _opcodeMap.Add(0x20, jumpRelativeIfNotZero);
            _opcodeMap.Add(0x21, (x) => HL = loadImmediateWord());
            _opcodeMap.Add(0x24, (x) => H = increment(H));
            _opcodeMap.Add(0x25, (x) => H = decrement(H));
            _opcodeMap.Add(0x26, (x) => H = loadImmediate());
            _opcodeMap.Add(0x29, (x) => HL = add(HL, HL));
            _opcodeMap.Add(0x2C, (x) => L = increment(L));
            _opcodeMap.Add(0x2D, (x) => L = decrement(L));
            _opcodeMap.Add(0x2E, (x) => L = loadImmediate());
            _opcodeMap.Add(0x31, (x) => SP = loadImmediateWord());
            _opcodeMap.Add(0x32, saveDecrement);
            _opcodeMap.Add(0x3C, (x) => A = increment(A));
            _opcodeMap.Add(0x3D, (x) => A = decrement(A));
            _opcodeMap.Add(0x3E, (x) => A = loadImmediate());
            _opcodeMap.Add(0xC3, jump);

            _opcodeExtendedMap = new Dictionary<byte, Action<byte>>();
            _opcodeExtendedMap.Add(0x40, map4XOperations);
            _opcodeExtendedMap.Add(0x50, map5XOperations);
            _opcodeExtendedMap.Add(0x60, map6XOperations);
            _opcodeExtendedMap.Add(0x70, map7XOperations);
            _opcodeExtendedMap.Add(0xA0, mapAXOperations);
            _opcodeExtendedMap.Add(0xB0, mapBXOperations);

            _opcodeMap4X = new Dictionary<byte, Action>();
            _opcodeMap4X.Add(0x00, () => B = load(B));
            _opcodeMap4X.Add(0x01, () => B = load(C));
            _opcodeMap4X.Add(0x02, () => B = load(D));
            _opcodeMap4X.Add(0x03, () => B = load(E));
            _opcodeMap4X.Add(0x04, () => B = load(H));
            _opcodeMap4X.Add(0x05, () => B = load(L));
            _opcodeMap4X.Add(0x06, () => B = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap4X.Add(0x07, () => B = load(A));
            _opcodeMap4X.Add(0x08, () => C = load(B));
            _opcodeMap4X.Add(0x09, () => C = load(C));
            _opcodeMap4X.Add(0x0A, () => C = load(D));
            _opcodeMap4X.Add(0x0B, () => C = load(E));
            _opcodeMap4X.Add(0x0C, () => C = load(H));
            _opcodeMap4X.Add(0x0D, () => C = load(L));
            _opcodeMap4X.Add(0x0E, () => C = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap4X.Add(0x0F, () => C = load(A));

            _opcodeMap5X = new Dictionary<byte, Action>();
            _opcodeMap5X.Add(0x00, () => D = load(B));
            _opcodeMap5X.Add(0x01, () => D = load(C));
            _opcodeMap5X.Add(0x02, () => D = load(D));
            _opcodeMap5X.Add(0x03, () => D = load(E));
            _opcodeMap5X.Add(0x04, () => D = load(H));
            _opcodeMap5X.Add(0x05, () => D = load(L));
            _opcodeMap5X.Add(0x06, () => D = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap5X.Add(0x07, () => D = load(A));
            _opcodeMap5X.Add(0x08, () => E = load(B));
            _opcodeMap5X.Add(0x09, () => E = load(C));
            _opcodeMap5X.Add(0x0A, () => E = load(D));
            _opcodeMap5X.Add(0x0B, () => E = load(E));
            _opcodeMap5X.Add(0x0C, () => E = load(H));
            _opcodeMap5X.Add(0x0D, () => E = load(L));
            _opcodeMap5X.Add(0x0E, () => E = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap5X.Add(0x0F, () => E = load(A));

            _opcodeMap6X = new Dictionary<byte, Action>();
            _opcodeMap6X.Add(0x00, () => H = load(B));
            _opcodeMap6X.Add(0x01, () => H = load(C));
            _opcodeMap6X.Add(0x02, () => H = load(D));
            _opcodeMap6X.Add(0x03, () => H = load(E));
            _opcodeMap6X.Add(0x04, () => H = load(H));
            _opcodeMap6X.Add(0x05, () => H = load(L));
            _opcodeMap6X.Add(0x06, () => H = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap6X.Add(0x07, () => H = load(A));
            _opcodeMap6X.Add(0x08, () => L = load(B));
            _opcodeMap6X.Add(0x09, () => L = load(C));
            _opcodeMap6X.Add(0x0A, () => L = load(D));
            _opcodeMap6X.Add(0x0B, () => L = load(E));
            _opcodeMap6X.Add(0x0C, () => L = load(H));
            _opcodeMap6X.Add(0x0D, () => L = load(L));
            _opcodeMap6X.Add(0x0E, () => L = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap6X.Add(0x0F, () => L = load(A));

            _opcodeMap7X = new Dictionary<byte, Action>();
            _opcodeMap7X.Add(0x07, () => GameBoy.ROM.WriteByte(HL, A));
            _opcodeMap7X.Add(0x08, () => A = load(B));
            _opcodeMap7X.Add(0x09, () => A = load(C));
            _opcodeMap7X.Add(0x0A, () => A = load(D));
            _opcodeMap7X.Add(0x0B, () => A = load(E));
            _opcodeMap7X.Add(0x0C, () => A = load(H));
            _opcodeMap7X.Add(0x0D, () => A = load(L));
            _opcodeMap7X.Add(0x0E, () => A = load(GameBoy.ROM.ReadByte(HL)));
            _opcodeMap7X.Add(0x0F, () => A = load(A));

            _opcodeMapAX = new Dictionary<byte, Action>();
            _opcodeMapAX.Add(0x08, () => xor(B));
            _opcodeMapAX.Add(0x09, () => xor(C));
            _opcodeMapAX.Add(0x0A, () => xor(D));
            _opcodeMapAX.Add(0x0B, () => xor(E));
            _opcodeMapAX.Add(0x0C, () => xor(H));
            _opcodeMapAX.Add(0x0D, () => xor(L));
            _opcodeMapAX.Add(0x0E, () => xor(GameBoy.ROM.ReadByte(HL)));
            _opcodeMapAX.Add(0x0F, () => xor(A));

            _opcodeMapBX = new Dictionary<byte, Action>();
            _opcodeMapBX.Add(0x00, () => or(B));
            _opcodeMapBX.Add(0x01, () => or(C));
            _opcodeMapBX.Add(0x02, () => or(D));
            _opcodeMapBX.Add(0x03, () => or(E));
            _opcodeMapBX.Add(0x04, () => or(H));
            _opcodeMapBX.Add(0x05, () => or(L));
            _opcodeMapBX.Add(0x06, () => or(GameBoy.ROM.ReadByte(HL)));
            _opcodeMapBX.Add(0x07, () => or(A));
            _opcodeMapBX.Add(0x08, () => compare(B));
            _opcodeMapBX.Add(0x09, () => compare(C));
            _opcodeMapBX.Add(0x0A, () => compare(D));
            _opcodeMapBX.Add(0x0B, () => compare(E));
            _opcodeMapBX.Add(0x0C, () => compare(H));
            _opcodeMapBX.Add(0x0D, () => compare(L));
            _opcodeMapBX.Add(0x0E, () => compare(GameBoy.ROM.ReadByte(HL)));
            _opcodeMapBX.Add(0x0F, () => compare(A));
        }

        public void Start()
        {
            PC = 0x0100;

            /*
            // 00
            byte tByte = GameBoy.ROM.ReadByte(PC);
            PC++;
            Debug.Log(tByte.ToString("x"));

            // JMP
            tByte = GameBoy.ROM.ReadByte(PC);
            PC++;
            Debug.Log(tByte.ToString("x"));

            // 0150
            ushort tShort = GameBoy.ROM.ReadWord(PC);
            PC = tShort;
            Debug.Log(tShort.ToString("x"));

            // JMP
            tByte = GameBoy.ROM.ReadByte(PC);
            PC++;
            Debug.Log(tByte.ToString("x"));

            // 020C
            tShort = GameBoy.ROM.ReadWord(PC);
            PC = tShort;
            Debug.Log(tShort.ToString("x"));

            // XOR A
            tByte = GameBoy.ROM.ReadByte(PC);
            PC++;
            Debug.Log(tByte.ToString("x"));

            tByte = GameBoy.ROM.ReadByte(PC);
            PC++;
            Debug.Log(tByte.ToString("x"));
            */
        }

        public void Update()
        {
            byte opcode = readByte();

            Debug.Log(opcode.ToString("x"));

            processOpcode(opcode);

            //ushort opcode = (ushort)((_memory[_pc] << 8) | _memory[_pc + 1]);
            //_pc += OPCODE_SIZE;

            //Chip8.Operations.ProcessOpcode(opcode);
        }

        #endregion

        #region Process

        private void processOpcode(byte opcode)
        {
            if (_opcodeMap.ContainsKey(opcode))
            {
                _opcodeMap[opcode](opcode);
            }
            else
            {
                byte maskedOpcode = (byte)(opcode & 0xF0);
                _opcodeExtendedMap[maskedOpcode](opcode);
            }
        }

        #endregion

        #region Mapping

        private void map4XOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMap4X[opcodeLSN]();
        }

        private void map5XOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMap5X[opcodeLSN]();
        }

        private void map6XOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMap6X[opcodeLSN]();
        }

        private void map7XOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMap7X[opcodeLSN]();
        }

        private void mapAXOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMapAX[opcodeLSN]();
        }

        private void mapBXOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMapBX[opcodeLSN]();
        }

        #endregion

        #region Operations

        private void noOperation(byte opcode)
        {
            Debug.Log("No Operation");
        }

        private byte decrement(byte value)
        {
            Debug.Log("Decrement");

            byte returnValue = value--;

            Zf = returnValue == 0x00;
            Nf = true;
            Hf = (value & 0x0F) == 0x00;

            return (returnValue);
        }

        private byte increment(byte value)
        {
            Debug.Log("Increment");

            byte returnValue = value++;

            Zf = returnValue == 0x00;
            Nf = false;
            Hf = (value & 0x0F) == 0x0F;

            return (returnValue);
        }

        private void saveDecrement(byte opcode)
        {
            Debug.Log("Save Decrement");

            GameBoy.ROM.WriteByte(HL, A);
            HL--;
        }

        private void or(byte value)
        {
            Debug.Log("OR");

            A |= value;

            Zf = A == 0x00;
            Nf = false;
            Hf = false;
            Cf = false;
        }

        private void xor(byte value)
        {
            Debug.Log("XOR");

            A ^= value;

            Zf = A == 0x00;
            Nf = false;
            Hf = false;
            Cf = false;
        }

        private void compare(byte value)
        {
            Debug.Log("Compare");

            Zf = A == value;
            Nf = true;
            Hf = (A & 0x0F) < (value & 0x0F);
            Cf = value > A;
        }

        private ushort add(ushort augend, ushort addend)
        {
            Debug.Log("Add");

            ushort halfSum = (ushort)((augend & 0x00FF) + (addend & 0x00FF));
            uint sum = (uint)augend + (uint)addend;

            Nf = false;
            Hf = halfSum > 0x00FF;
            Cf = (sum & 0xFFFF0000) > 0x00000000;

            return ((ushort)(sum & 0x0000FFFF));
        }

        #region Load

        private byte load(byte value)
        {
            Debug.Log("Load");

            return (value);
        }

        private byte loadImmediate()
        {
            Debug.Log("Load Immediate");

            return (readByte());
        }

        private ushort loadImmediateWord()
        {
            Debug.Log("Load Immediate Word");

            return (readWord());
        }

        #endregion

        #region Jump

        private void jump(byte opcode)
        {
            Debug.Log("Jump");

            PC = GameBoy.ROM.ReadWord(PC);
        }

        private void jumpRelative(byte opcode)
        {
            Debug.Log("Jump Relative");

            byte relative = GameBoy.ROM.ReadByte(PC);

            PC += (byte)((relative ^ 0x80) - 0x80);
        }

        private void jumpRelativeIfNotZero(byte opcode)
        {
            Debug.Log("Jump Relative If Not Zero");

            if (Zf)
                PC++;
            else
                jumpRelative(opcode);
        }

        #endregion

        #region Rotation

        private void rotateALeft(byte opcode)
        {
            Debug.Log("Rotate A Left");

            byte msb = (byte)(A >> 7);

            A = (byte)((byte)(A << 1) | msb);

            Nf = false;
            Hf = false;
            Cf = msb == 1;
        }

        private void rotateARightThroughCarry(byte opcode)
        {
            Debug.Log("Rotate A Right Through Carry");

            byte carryBit = Cf ? (byte)0x80 : (byte)0x00;

            Cf = (A & 0x01) == 0x01;

            A = (byte)(carryBit | (A >> 1));

            Nf = false;
            Hf = false;
        }

        #endregion

        #endregion

        #region I/O

        private byte readByte()
        {
            byte readByte = GameBoy.ROM.ReadByte(PC);
            PC++;

            return (readByte);
        }

        private ushort readWord()
        {
            ushort word = GameBoy.ROM.ReadWord(PC);
            PC += 2;

            return (word);
        }

        #endregion
    }
}
