using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using MappingConfigNamespace;

namespace InteractorMidi
{
    public abstract class InteractorMidiInput
    {
        public MappingConfig mappingConfig = new MappingConfig();
        private static InputDevice inputDevice = null;

        public List<String> retrieveInputDeviceKeys()
        {
            List<String> result = new List<String>();
            foreach (InputDevice tmp in InputDevice.GetAll())
            {
                String inputDeviceKey = tmp.Id + "_" + tmp.Name;
                Debug.WriteLine(inputDeviceKey);
                result.Add(inputDeviceKey);
            }
            return result;
        }

        public void start(MappingConfig mappingConfig)
        {
            this.mappingConfig = mappingConfig;
            
            foreach (InputDevice tmp in InputDevice.GetAll())
            {
                String inputDeviceKeyTmp = tmp.Id + "_" + tmp.Name;
                if (!inputDeviceKeyTmp.Equals(mappingConfig.inputDeviceKey))
                {
                    continue;
                }

                if(null == inputDevice)
                {
                    inputDevice = tmp;
                    inputDevice.EventReceived += MidiEventReceived;
                    inputDevice.StartEventsListening();
                    //(inputDevice as IDisposable)?.Dispose();
                }

                break;
            }
        }

        public void MidiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
           var midiDevice = (MidiDevice)sender;
           Debug.WriteLine($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");

            String eventType = e.Event.EventType.ToString();
            int channel = 0;
            int noteNumber = 0;
            int velocity = 0;
            switch (e.Event.EventType)
            {
                case MidiEventType.NoteOn:
                {
                    NoteOnEvent midiEvent = (NoteOnEvent)e.Event;
                    channel = midiEvent.Channel;
                    noteNumber = midiEvent.NoteNumber;
                    velocity = midiEvent.Velocity;
                    break;
                }
                case MidiEventType.NoteOff:
                { 
                    NoteOffEvent midiEvent = (NoteOffEvent)e.Event;
                    channel = midiEvent.Channel;
                    noteNumber = midiEvent.NoteNumber;
                    velocity = midiEvent.Velocity;
                    break;
                }
            }

            MidiEventReceived(eventType, channel, noteNumber, velocity);
        }

        public abstract void MidiEventReceived(String eventType, int channel, int noteNumber, int velocity);
    }
}