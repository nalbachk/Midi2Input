using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace InteractorMidi
{
    public abstract class InteractorMidiInput
    {
        private static InputDevice inputDevice;

        public void start()
        {
            foreach (var tmp in InputDevice.GetAll())
            {
                inputDevice = tmp;
                Debug.WriteLine(inputDevice.Name);
                inputDevice.EventReceived += MidiEventReceived;
                inputDevice.StartEventsListening();

                //(inputDevice as IDisposable)?.Dispose();
                break;
            }
        }

        public void MidiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var midiDevice = (MidiDevice)sender;
            Debug.WriteLine($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
           if(MidiEventType.NoteOn == e.Event.EventType)
           {
                NoteOnEvent noteOnEvent = (NoteOnEvent) e.Event;
                MidiNoteOnReceived(noteOnEvent.NoteNumber);
           }
        }

        public abstract void MidiNoteOnReceived(int noteNumber);
    }
}