using NWaves.DemoUwpEffect;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Audio;
using Windows.Media.Effects;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace NWaves.DemoUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AudioGraph graph;
        private AudioFileInputNode fileInputNode;
        private AudioDeviceOutputNode deviceOutputNode;

        public float Freq { get; set; }
        public float Q { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void File_Click(object sender, RoutedEventArgs e)
        {
            if (graph == null)
            {
                await CreateAudioGraph();
            }

            await SelectInputFile();
        }

        private async Task SelectInputFile()
        {
            if (fileInputNode != null)
            {
                fileInputNode.Dispose();
                
                if (graphButton.Content.Equals("Stop Graph"))
                {
                    TogglePlay();
                }
            }

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.FileTypeFilter.Add(".mp3");
            filePicker.FileTypeFilter.Add(".wav");
            filePicker.FileTypeFilter.Add(".wma");
            filePicker.FileTypeFilter.Add(".m4a");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            CreateAudioFileInputNodeResult fileInputNodeResult = await graph.CreateFileInputNodeAsync(file);
            if (fileInputNodeResult.Status != AudioFileNodeCreationStatus.Success)
            {
                NotifyUser(String.Format("Cannot read input file because {0}", fileInputNodeResult.Status.ToString()), NotifyType.ErrorMessage);
                return;
            }

            fileInputNode = fileInputNodeResult.FileInputNode;
            fileInputNode.AddOutgoingConnection(deviceOutputNode);
            fileButton.Background = new SolidColorBrush(Colors.Green);

            fileInputNode.FileCompleted += FileInput_FileCompleted;

            graphButton.IsEnabled = true;

            AddCustomEffect();
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            TogglePlay();
        }

        private void TogglePlay()
        {
            if (graphButton.Content.Equals("Start Graph"))
            {
                graph.Start();
                graphButton.Content = "Stop Graph";
                audioPipe.Fill = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                graph.Stop();
                graphButton.Content = "Start Graph";
                audioPipe.Fill = new SolidColorBrush(Color.FromArgb(255, 49, 49, 49));
            }
        }

        private async Task CreateAudioGraph()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                NotifyUser(String.Format("AudioGraph Creation Error because {0}", result.Status.ToString()), NotifyType.ErrorMessage);
                return;
            }

            graph = result.Graph;

            graph.EncodingProperties.SampleRate = 44100;

            CreateAudioDeviceOutputNodeResult deviceOutputResult = await graph.CreateDeviceOutputNodeAsync();

            if (deviceOutputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                NotifyUser(String.Format("Audio Device Output unavailable because {0}", deviceOutputResult.Status.ToString()), NotifyType.ErrorMessage);
                speakerContainer.Background = new SolidColorBrush(Colors.Red);
                return;
            }

            deviceOutputNode = deviceOutputResult.DeviceOutputNode;
            NotifyUser("Device Output Node successfully created", NotifyType.StatusMessage);
            speakerContainer.Background = new SolidColorBrush(Colors.Green);
        }

        private void AddCustomEffect()
        {
            PropertySet wahwahProperties = new PropertySet
            {
                { "Max frequency", 2000f },
                { "Q", 0.5f }
            };

            AudioEffectDefinition wahwahDefinition = new AudioEffectDefinition(typeof(NWavesEffect).FullName, wahwahProperties);
            fileInputNode.EffectDefinitions.Add(wahwahDefinition);
        }

        /// <summary>
        /// Event handler for file completion event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void FileInput_FileCompleted(AudioFileInputNode sender, object args)
        {
            // File playback is done. Stop the graph
            graph.Stop();

            // Reset the file input node so starting the graph will resume playback from beginning of the file
            sender.Reset();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NotifyUser("End of file reached", NotifyType.StatusMessage);
                graphButton.Content = "Start Graph";
            });
        }


        /// <summary>
        /// Display a message to the user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void NotifyUser(string message, NotifyType type)
        {
            if (Dispatcher.HasThreadAccess)
            {
                UpdateStatus(message, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(message, type));
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBlock.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBlock.Foreground = new SolidColorBrush(Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            StatusBlock.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;

            if (StatusBlock.Text != String.Empty)
            {
                StatusBlock.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBlock.Visibility = Visibility.Collapsed;
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        private void MaxFreqChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (fileInputNode != null)
            {
                fileInputNode.EffectDefinitions.Last().Properties["Max frequency"] = (float)e.NewValue;
            }
        }

        private void QChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (fileInputNode != null)
            {
                fileInputNode.EffectDefinitions.Last().Properties["Q"] = (float)e.NewValue;
            }
        }
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };
}
