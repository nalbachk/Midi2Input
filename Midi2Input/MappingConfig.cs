using System;
using System.Collections.Generic;

namespace MappingConfigNamespace
{
    public class MappingConfig
    {
        public String inputDeviceKey;
        public List<MappingEntry> mappingEntries = new List<MappingEntry>();

        public void initExample1()
        {
            MappingEntry mappingEntry;
            
            mappingEntry = new MappingEntry();
            mappingEntry.noteNumber = 45;
            mappingEntry.keyScanCode = 0x25; // K
            mappingEntries.Add(mappingEntry);

            mappingEntry = new MappingEntry();
            mappingEntry.noteNumber = 43;
            mappingEntry.keyScanCode = 0x24; // J
            mappingEntries.Add(mappingEntry);

            mappingEntry = new MappingEntry();
            mappingEntry.noteNumber = 38;
            mappingEntry.keyScanCode = 0x21; // F
            mappingEntries.Add(mappingEntry);

            mappingEntry = new MappingEntry();
            mappingEntry.noteNumber = 48;
            mappingEntry.keyScanCode = 0x20; // D
            mappingEntries.Add(mappingEntry);
        }
    }

    public class MappingEntry
    {
        public String eventType = "NoteOn";
        public int channel = -1;
        public int noteNumber = -1;
        public int velocity = -1;
        public String action = "sendKeyDownAndUpAsInput";
        public ushort keyScanCode = 0x00;
        public int duration = 50;
    }
}
