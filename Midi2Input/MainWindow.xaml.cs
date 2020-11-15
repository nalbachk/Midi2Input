using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;
using InteractorMidi;

namespace Midi2Input
{
    class InteractorUser32
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

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

        public void sendKeyDownAndUpAsInput(ushort keyScanCode, int duration) // 0x11==W
        {
            sendKeyDownAsInput(keyScanCode);
            Thread.Sleep(duration);
            sendKeyUpAsInput(keyScanCode);
        }

        public void sendKeyDownAsInput(ushort keyScanCode) // 0x11==W
        {
            Input[] inputs = new Input[]
            {
                new Input
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
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        public void sendKeyUpAsInput(ushort keyScanCode) // 0x11==W
        {
            Input[] inputs = new Input[]
            {
                new Input
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
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }
    }

    public class MyInteractorMidiInput : InteractorMidiInput
    {
        private InteractorUser32 interactorUser32 = new InteractorUser32();

        public override void MidiNoteOnReceived(int noteNumber)
        {
            ushort keyScanCode = 0x00;
            switch (noteNumber)
            {
                case 45: keyScanCode = 0x25;  break; // K
                case 43: keyScanCode = 0x24; break; // J
                case 38: keyScanCode = 0x21; break; // F
                case 48: keyScanCode = 0x20; break; // D
            }
            
            if(keyScanCode > 0x00)
            {
                interactorUser32.sendKeyDownAndUpAsInput(keyScanCode, 50);
            }
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InteractorMidiInput interactorMidiInput = new MyInteractorMidiInput();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello.");
            interactorMidiInput.start();

            /*
            for (int i = 0; i < 10; i++) {
                Thread.Sleep(5000);
                interactorUser32.sendKeyDownAndUpAsInput(0x24, 50); // J
            }
            */
        }

    }
}
