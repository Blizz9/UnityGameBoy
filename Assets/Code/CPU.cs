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
        private Dictionary<byte, Action> _opcodeMapAX;

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
            _opcodeMap.Add(0x06, (x) => B = loadImmediate());
            _opcodeMap.Add(0x0E, (x) => C = loadImmediate());
            _opcodeMap.Add(0x11, (x) => DE = loadImmediateWord());
            _opcodeMap.Add(0x16, (x) => D = loadImmediate());
            _opcodeMap.Add(0x1E, (x) => E = loadImmediate());
            _opcodeMap.Add(0x21, (x) => HL = loadImmediateWord());
            _opcodeMap.Add(0x26, (x) => H = loadImmediate());
            _opcodeMap.Add(0x2E, (x) => L = loadImmediate());
            _opcodeMap.Add(0x31, (x) => SP = loadImmediateWord());
            _opcodeMap.Add(0x32, saveDecrement);
            _opcodeMap.Add(0x3E, (x) => A = loadImmediate());
            _opcodeMap.Add(0xC3, jump);

            _opcodeExtendedMap = new Dictionary<byte, Action<byte>>();
            _opcodeExtendedMap.Add(0xA0, mapAXOperations);

            _opcodeMapAX = new Dictionary<byte, Action>();
            _opcodeMapAX.Add(0x08, () => xor(B));
            _opcodeMapAX.Add(0x09, () => xor(C));
            _opcodeMapAX.Add(0x0A, () => xor(D));
            _opcodeMapAX.Add(0x0B, () => xor(E));
            _opcodeMapAX.Add(0x0C, () => xor(H));
            _opcodeMapAX.Add(0x0D, () => xor(L));
            _opcodeMapAX.Add(0x0E, () => xor(GameBoy.ROM.ReadByte(HL)));
            _opcodeMapAX.Add(0x0F, () => xor(A));
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

        private void mapAXOperations(byte opcode)
        {
            byte opcodeLSN = (byte)(opcode & 0x0F);
            _opcodeMapAX[opcodeLSN]();
        }

        #endregion

        #region Operations

        private void noOperation(byte opcode)
        {
            Debug.Log("No Operation");
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

        private void saveDecrement(byte opcode)
        {
            Debug.Log("Save Decrement");

            GameBoy.ROM.WriteByte(HL, A);
            HL--;
        }

        private void xor(byte value)
        {
            Debug.Log("XOR");

            A ^= value;

            Zf = A == 0x00;
            Cf = false;
            Nf = false;
            Hf = false;
        }

        private void jump(byte opcode)
        {
            Debug.Log("Jump");

            PC = GameBoy.ROM.ReadWord(PC);
        }

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
