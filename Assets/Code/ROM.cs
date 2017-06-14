using System.IO;
using System.Text;
using com.PixelismGames.UnityGameBoy.Enumerations;
using com.PixelismGames.UnityGameBoy.Utilities;
using UnityEngine;

namespace com.PixelismGames.UnityGameBoy
{
    [AddComponentMenu("Pixelism Games/ROM")]
    public class ROM : MonoBehaviour
    {
        private byte[] _rom;

        [ReadOnly] public string Title;
        [ReadOnly] public ColorType ColorType;
        [ReadOnly] public int NewLicenseeCode; // make this an enumeration
        [ReadOnly] public bool SupportsSuperGameBoyFucntionality;
        [ReadOnly] public CartridgeType CartridgeType;
        [ReadOnly] public int Size;
        [ReadOnly] public int RAMSize;
        [ReadOnly] public bool IsJapanese;
        [ReadOnly] public int OldLicenseeCode; // make this an enumeration
        [ReadOnly] public byte MaskROMVersion;
        [ReadOnly] public byte HeaderChecksum;
        [ReadOnly] public byte CalculatedHeaderChecksum;
        [ReadOnly] public ushort Checksum;
        [ReadOnly] public ushort CalculatedChecksum;

        #region MonoBehaviour

        public void Awake()
        {
            _rom = File.ReadAllBytes("./Contrib/ROMs/tetris.gb");

            StringBuilder titleString = new StringBuilder();
            for (int i = 0x0134; i <= 0x0142; i++)
            {
                if (_rom[i] == 0x00)
                    break;

                titleString.Append((char)_rom[i]);
            }
            Title = titleString.ToString();

            if ((_rom[0x0143] & 0xC0) == 0xC0)
                ColorType = ColorType.ColorOnly;
            else if ((_rom[0x0143] & 0x80) == 0x80)
                ColorType = ColorType.MonochromeOrColor;
            else
                ColorType = ColorType.Monochrome;

            NewLicenseeCode = (_rom[0x0144] << 4) | _rom[0x0145];

            SupportsSuperGameBoyFucntionality = _rom[0x0146] == 0x03;

            CartridgeType = (CartridgeType)_rom[0x0147];

            Size = (32 * 1024) << _rom[0x0148];

            switch (_rom[0x0149])
            {
                case 0x01:
                    RAMSize = 2048;
                    break;

                case 0x02:
                    RAMSize = 8192;
                    break;

                case 0x03:
                    RAMSize = 32768;
                    break;

                case 0x04:
                    RAMSize = 131072;
                    break;

                case 0x05:
                    RAMSize = 65536;
                    break;

                default:
                    RAMSize = 0;
                    break;
            }

            IsJapanese = _rom[0x014A] == 0x00;

            OldLicenseeCode = _rom[0x014B];

            MaskROMVersion = _rom[0x014C];

            HeaderChecksum = _rom[0x014D];

            int calculatedHeaderChecksum = 0;
            for (int i = 0x0134; i <= 0x014C; i++)
                calculatedHeaderChecksum = calculatedHeaderChecksum - _rom[i] - 1;
            CalculatedHeaderChecksum = (byte)(calculatedHeaderChecksum & 0xFF);

            Checksum = (ushort)((_rom[0x014E] << 8) | _rom[0x014F]);

            int calculatedChecksum = 0;
            for (int i = 0; i < _rom.Length; i++)
                if ((i != 0x014E) && (i != 0x014F))
                    calculatedChecksum += _rom[i];
            CalculatedChecksum = (ushort)(calculatedChecksum & 0xFFFF);
        }

        #endregion

        #region I/O

        public byte ReadByte(ushort address)
        {
            return (_rom[address & 0x7FFF]);
        }

        public ushort ReadWord(ushort address)
        {
            ushort low = ReadByte(address);
            ushort high = ReadByte((ushort)(address + 1));
            return ((ushort)((high << 8) | low));
        }

        #endregion
    }
}
