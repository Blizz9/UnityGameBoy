using com.PixelismGames.UnityGameBoy.Utilities;
using UnityEngine;

namespace com.PixelismGames.UnityGameBoy
{
    [AddComponentMenu("Pixelism Games/CPU")]
    public class CPU : MonoBehaviour
    {
        [ReadOnly] public ushort PC;

        #region MonoBehaviour

        public void Start()
        {
            PC = 0x0100;

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
        }

        #endregion
    }
}
