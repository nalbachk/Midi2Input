using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InteractorMidi;
using MappingConfigNamespace;
using Newtonsoft.Json;
using System.IO;

namespace Midi2Input
{

    public class MyInteractorMidiInput : InteractorMidiInput
    {
        private InteractorUser32 interactorUser32 = new InteractorUser32();

        public override void MidiEventReceived(String eventType, int channel, int noteNumber, int velocity)
        {
            if(mappingConfig.logMidiEvents)
            {
                viewLogger.log("eventType:" + eventType + ", channel:" + channel + ", noteNumber:" + noteNumber + ", velocity:" + velocity);
            }

            Parallel.ForEach(mappingConfig.mappingEntries, (mappingEntry) =>
            {
                if (mappingEntry.channel > -1 && channel != mappingEntry.channel)
                {
                    return;
                }

                if (mappingEntry.noteNumber > -1 && noteNumber != mappingEntry.noteNumber)
                {
                    return;
                }

                if (mappingEntry.velocity > -1 && velocity != mappingEntry.velocity)
                {
                    return;
                }

                if (!eventType.Equals(mappingEntry.eventType))
                {
                    return;
                }
                
                if ("sendKeyDownAndUpAsInput".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyDownAndUpAsInput(mappingEntry.keyScanCode, mappingEntry.duration);
                }
                else if ("sendKeyDownAsInput".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyDownAsInput(mappingEntry.keyScanCode);
                }
                else if ("sendKeyUpAsInput".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyUpAsInput(mappingEntry.keyScanCode);
                }
                else if("sendKeyDownAndUpAsKeybdEvent".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyDownAndUpAsKeybdEvent(mappingEntry.keyScanCode, mappingEntry.duration);
                }
                else if ("sendKeyDownAsKeybdEvent".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyDownAsKeybdEvent(mappingEntry.keyScanCode);
                }
                else if ("sendKeyUpAsKeybdEvent".Equals(mappingEntry.action))
                {
                    interactorUser32.sendKeyUpAsKeybdEvent(mappingEntry.keyScanCode);
                }
            });
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewLogger viewLogger = null;
        private InteractorMidiInput interactorMidiInput = null;

        public MainWindow()
        {
            InitializeComponent();
            this.viewLogger = new ViewLogger(TxtLog);
            interactorMidiInput = new MyInteractorMidiInput();
            interactorMidiInput.viewLogger = viewLogger;
            BtnLoad_Click(null, null);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            List<String> inputDeviceKeys = interactorMidiInput.retrieveInputDeviceKeys();
            if (inputDeviceKeys.Count < 1)
            {
                viewLogger.log("no InputDevices");
                return;
            }

            String fileName = "mapping_config.txt";
            String jsonMappingConfig = "";
            
            if (File.Exists(fileName))
            {
                viewLogger.log("read MappingConfig: " + fileName);
                jsonMappingConfig = File.ReadAllText(fileName);
            }
            else
            {
                MappingConfig config = new MappingConfig();
                config.inputDeviceKey = inputDeviceKeys.First();
                config.initExample1();
                jsonMappingConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
            }
            TxtConfig.Clear();
            TxtConfig.Text = jsonMappingConfig;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            List<String> inputDeviceKeys = interactorMidiInput.retrieveInputDeviceKeys();
            if (inputDeviceKeys.Count < 1)
            {
                viewLogger.log("no InputDevices");
                return;
            }

            String fileName = "mapping_config.txt";
            MappingConfig config;
            {
                String jsonMappingConfig = TxtConfig.Text;
                if (jsonMappingConfig.Length < 1)
                {
                    viewLogger.log("no MappingConfig => please load first");
                    return;
                }
                Type type = Type.GetType("MappingConfigNamespace.MappingConfig");
                config = (MappingConfig)JsonConvert.DeserializeObject(jsonMappingConfig, type);
            }

            if (null == config.inputDeviceKey || config.inputDeviceKey.Length < 1)
            {
                config.inputDeviceKey = inputDeviceKeys.First();
            }
            if (!inputDeviceKeys.Contains(config.inputDeviceKey))
            {
                config.inputDeviceKey = inputDeviceKeys.First();
            }

            viewLogger.log("write MappingConfig: " + fileName);
            {
                String jsonMappingConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
                TxtConfig.Clear();
                TxtConfig.Text = jsonMappingConfig;
                File.WriteAllText(fileName, jsonMappingConfig);
            }

            viewLogger.log("inputDeviceKey: " + config.inputDeviceKey);
            interactorMidiInput.start(config);
        }
    }
}
