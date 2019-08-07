using NWaves.DemoXamarin.DependencyServices;
using System;
using Xamarin.Forms;

namespace NWaves.DemoXamarin
{
	public partial class MainPage : ContentPage
	{
        private IAudioService _audioService;

        private bool _isRecording;

        private Label _pitchZcrLabel, _pitchAutoCorrLabel;
        private Button _recordButton;


        public MainPage()
        {
            InitializeComponent();

            _audioService = DependencyService.Get<IAudioService>();
            _audioService.PitchEstimated += UpdatePitch;

            _pitchZcrLabel = this.FindByName<Label>("pitchZcrLabel");
            _pitchAutoCorrLabel = this.FindByName<Label>("pitchAutoCorrLabel");

            _recordButton = this.FindByName<Button>("recordButton");
        }

        private void UpdatePitch(object sender, PitchEstimatedEventArgs e)
        {
            _pitchZcrLabel.Text = $"ZCR: {e.PitchZcr: 0.#} Hz";
            _pitchAutoCorrLabel.Text = $"Autocorrelation: {e.PitchAutoCorr: 0.#} Hz";
        }

        void OnRecordClicked(object sender, EventArgs args)
        {
            if (_isRecording)
            {
                _audioService.StopRecording();
                _isRecording = false;
                _recordButton.Text = "Rec";
            }
            else
            {
                _audioService.StartRecording();
                _isRecording = true;
                _recordButton.Text = "Stop";
            }
        }
    }
}
