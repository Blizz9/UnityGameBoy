using Unity.Linq;
using UnityEngine;

namespace com.PixelismGames.UnityGameBoy
{
    [AddComponentMenu("Pixelism Games/Game Boy")]
    public class GameBoy : MonoBehaviour
    {
        private static ROM _rom;
        private static CPU _cpu;

        #region Singletons

        public static ROM ROM
        {
            get
            {
                if (_rom == null)
                {
                    Debug.Log("ERROR | Game Boy: ROM not yet provided");
                    Debug.Break();
                }

                return (_rom);
            }
        }

        public static CPU CPU
        {
            get
            {
                if (_cpu == null)
                {
                    Debug.Log("ERROR | Game Boy: CPU not yet provided");
                    Debug.Break();
                }

                return (_cpu);
            }
        }

        #endregion

        #region Provide Routines

        private static void provideROM(ROM rom)
        {
            if (_rom == null)
            {
                _rom = rom;
            }
            else
            {
                Debug.Log("ERROR | Game Boy: ROM already provided");
                Debug.Break();
            }
        }

        private static void provideCPU(CPU cpu)
        {
            if (_cpu == null)
            {
                _cpu = cpu;
            }
            else
            {
                Debug.Log("ERROR | Game Boy: CPU already provided");
                Debug.Break();
            }
        }

        #endregion

        #region MonoBehaviour

        public void Awake()
        {
            provideROM(gameObject.Children().OfComponent<ROM>().First());
            provideCPU(gameObject.Children().OfComponent<CPU>().First());
        }

        #endregion
    }
}
