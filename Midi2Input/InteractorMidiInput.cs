using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using MappingConfigNamespace;
using Midi2Input;
using System.Runtime.InteropServices;
using static InteractorMidi.NativeMethods;

namespace InteractorMidi
{
    internal static class NativeMethods
    {
        internal const int MMSYSERR_NOERROR = 0;
        internal const int CALLBACK_FUNCTION = 0x00030000;

        [StructLayout(LayoutKind.Sequential)]
        internal struct MidiInCaps
        {
            public short mid;
            public short pid;
            public int driverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string name;
            public int support;
        };

        internal delegate void MidiInProc(
            IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2);

        [DllImport("winmm.dll")]
        internal static extern int midiInGetNumDevs();

        [DllImport("winmm.dll")]
        internal static extern int midiInGetDevCaps(int deviceId, ref MidiInCaps caps, int sizeOfMidiInCaps);

        [DllImport("winmm.dll")]
        internal static extern int midiInClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInOpen(
            out IntPtr lphMidiIn,
            int uDeviceID,
            MidiInProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);

        [DllImport("winmm.dll")]
        internal static extern int midiInStart(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInStop(
            IntPtr hMidiIn);
    }

    public abstract class InteractorMidiInput
    {
        public ViewLogger viewLogger = null;
        public MappingConfig mappingConfig = new MappingConfig();

        private IntPtr inputDeviceHandle = IntPtr.Zero;
        private NativeMethods.MidiInProc midiInProc;

        public List<String> retrieveInputDeviceKeys()
        {
            List<String> result = new List<String>();

            int count = NativeMethods.midiInGetNumDevs();
            for(int deviceIndex=0; deviceIndex<count; deviceIndex++)
            {
                var caps = new MidiInCaps();
                int error = NativeMethods.midiInGetDevCaps(deviceIndex, ref caps, Marshal.SizeOf(caps));

                String inputDeviceKey = deviceIndex + "_" + caps.name;
                Debug.WriteLine(inputDeviceKey);
                result.Add(inputDeviceKey);
            }
            return result;
        }

        public void start(MappingConfig mappingConfig)
        {
            this.mappingConfig = mappingConfig;

            int count = NativeMethods.midiInGetNumDevs();
            int deviceIndexSelected = -1;
            for (int deviceIndex = 0; deviceIndex < count; deviceIndex++)
            {
                var caps = new MidiInCaps();
                int error = NativeMethods.midiInGetDevCaps(deviceIndex, ref caps, Marshal.SizeOf(caps));
                
                String inputDeviceKeyTmp = deviceIndex + "_" + caps.name;
                if (!inputDeviceKeyTmp.Equals(mappingConfig.inputDeviceKey))
                {
                    continue;
                }
                deviceIndexSelected = deviceIndex;
                break;
            }

            if(deviceIndexSelected > -1)
            {
                if(IntPtr.Zero == inputDeviceHandle)
                {
                    midiInProc = new NativeMethods.MidiInProc(MidiInEventProc);
                    bool opened = NativeMethods.midiInOpen(out inputDeviceHandle, deviceIndexSelected, midiInProc, IntPtr.Zero, NativeMethods.CALLBACK_FUNCTION) == NativeMethods.MMSYSERR_NOERROR;
                    bool startet = NativeMethods.midiInStart(inputDeviceHandle) == NativeMethods.MMSYSERR_NOERROR;
                }
            }
        }

        private void MidiInEventProc(
            IntPtr hMidiIn, 
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2)
        {
            Debug.WriteLine($"Event received from '{hMidiIn}' at {DateTime.Now}: {wMsg}, {dwInstance}, {dwParam1}, {dwParam2}");

            String eventType = "";
            int eventTypeNr = 0;
            int channel = 0;
            int noteNumber = 0;
            int velocity = 0;
            int other = 0;

            if (963 == wMsg)
            {
                // dwParam1==5058713 => 100 1101 0011 0000 1001 1001
                //          & 0x00F0 => 1001 0000 => 144 => NoteOn
                //          & 0x000F => 1001 => 9 => Channel
                //          (dwParam1 >> 8) => 0011 0000
                //           & 0xFF  => 0011 0000 => 48

                // dwParam2==9820 => 10 0110 0101 1100
                //          & 0x00FF => 0101 1100 => 92 => ?
                //          (dwParam2 >> 8) => 0010 0110
                //           & 0xFF  => 0010 0110 => 38

                dwParam1 = dwParam1 & 0xFFFF; // example:  12425, 12441
                eventTypeNr = dwParam1 & 0x00F0; // example: 128, 144
                channel = dwParam1 & 0x000F; // example: 9
                noteNumber = (dwParam1 >> 8) & 0xFF; // example: 48

                dwParam2 = dwParam2 & 0xFFFF; // example: 42687, 4799
                other = dwParam2 & 0x00FF; // 191?
                velocity = (dwParam2 >> 8) & 0xFF; //example (0-128): 166?, 18
            }

            switch(eventTypeNr)
            {
                case 128: eventType = "NoteOff"; break;
                case 144: eventType = "NoteOn"; break;
            }

            MidiEventReceived(eventType, channel, noteNumber, velocity);
        }

        public abstract void MidiEventReceived(String eventType, int channel, int noteNumber, int velocity);
    }
}