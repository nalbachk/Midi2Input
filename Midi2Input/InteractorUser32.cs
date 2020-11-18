using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Midi2Input
{
    class InteractorUser32
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        private bool isExtended(ushort keyScanCode)
        {
            if (((int)keyScanCode & 0xFF00) == 0xE000)
            { // extended key?
                return true;
            }
            return false;
        }

        public void sendKeyDownAndUpAsInput(ushort keyScanCode, int duration) // 0x11==W
        {
            sendKeyDownAsInput(keyScanCode);
            Thread.Sleep(duration);
            sendKeyUpAsInput(keyScanCode);
        }

        public void sendKeyDownAsInput(ushort keyScanCode) // 0x11==W
        {
            List<Input> inputList = new List<Input>();
            if (isExtended(keyScanCode))
            {
                inputList.Add(new Input
                {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = keyScanCode, // 0xE04B==left
                            dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode | KeyEventF.ExtendedKey),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                });
            }
            else
            {
                inputList.Add(new Input
                {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = keyScanCode, // 0x11==W
                            dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                });
            }
            

            Input[] inputArray = inputList.ToArray();
            SendInput((uint)inputArray.Length, inputArray, Marshal.SizeOf(typeof(Input)));
        }

        public void sendKeyUpAsInput(ushort keyScanCode) // 0x11==W
        {
            List<Input> inputList = new List<Input>();
            if (isExtended(keyScanCode))
            {
                inputList.Add(new Input
                {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = keyScanCode, // 0xE04B==left
                            dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode | KeyEventF.ExtendedKey),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                });
            }
            else
            {
                inputList.Add(new Input
                {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = keyScanCode, // 0x11==W
                            dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                });
            }

            Input[] inputArray = inputList.ToArray();
            SendInput((uint)inputArray.Length, inputArray, Marshal.SizeOf(typeof(Input)));
        }

        public const int KEYEVENTF_KEYDOWN = 0x0000; // New definition
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        // TODO not working atm
        public void sendKeyDownAndUpAsKeybdEvent(ushort keyScanCode, int duration) // 0x11==W
        {
            sendKeyDownAsKeybdEvent(keyScanCode);
            Thread.Sleep(duration);
            sendKeyUpAsKeybdEvent(keyScanCode);
        }

        public void sendKeyDownAsKeybdEvent(ushort keyScanCode) // 0x11==W
        {
            byte b = BitConverter.GetBytes(keyScanCode)[0];
            if (isExtended(keyScanCode))
            {
                keybd_event(0, b, KEYEVENTF_KEYDOWN | KEYEVENTF_EXTENDEDKEY, 0);
            }
            else
            {
                keybd_event(0, b, KEYEVENTF_KEYDOWN, 0);
            }
        }

        public void sendKeyUpAsKeybdEvent(ushort keyScanCode) // 0x11==W
        {
            byte b = BitConverter.GetBytes(keyScanCode)[0];
            if (isExtended(keyScanCode))
            {
                keybd_event(0, b, KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY, 0);
            }
            else
            {
                keybd_event(0, b, KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
