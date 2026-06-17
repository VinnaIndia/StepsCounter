using System.Windows.Input;

namespace StepsCounter.ViewModels
{
    public class MotionDetectionViewModel : BaseViewModel
    {
        private bool _isTracking;
        private bool _motionDetected;
        private double _threshold = 0.8;
        private System.Timers.Timer? _clearTimer;

        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                _isTracking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool MotionDetected
        {
            get => _motionDetected;
            set
            {
                _motionDetected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MotionIcon));
                OnPropertyChanged(nameof(MotionLabel));
                OnPropertyChanged(nameof(CircleColor));
                OnPropertyChanged(nameof(LabelColor));
            }
        }

        public double Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SensitivityLabel));
            }
        }

        public bool CanStart => !IsTracking;
        public bool CanStop => IsTracking;

        public string MotionIcon => MotionDetected ? "✅" : (IsTracking ? "📱" : "⬜");
        public string MotionLabel => MotionDetected ? "Motion Detected!" : (IsTracking ? "Waiting for movement..." : "Press Start");
        public string CircleColor => MotionDetected ? "#ECFDF5" : (IsTracking ? "#EEF2FF" : "#F3F4F6");
        public string LabelColor => MotionDetected ? "#10B981" : (IsTracking ? "#4F46E5" : "#9CA3AF");
        public string SensitivityLabel => $"Threshold: {Threshold:F1}";

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public MotionDetectionViewModel()
        {
            StartCommand = new Command(StartTracking);
            StopCommand = new Command(StopTracking);
        }

        private void StartTracking()
        {
            if (!Accelerometer.Default.IsSupported) return;

            Accelerometer.Default.ReadingChanged += OnAccelerometerReading;
            Accelerometer.Default.Start(SensorSpeed.UI);
            IsTracking = true;
            MotionDetected = false;
        }

        private void StopTracking()
        {
            _clearTimer?.Stop();
            _clearTimer?.Dispose();

            Accelerometer.Default.Stop();
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReading;
            IsTracking = false;
            MotionDetected = false;
        }

        private void OnAccelerometerReading(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading.Acceleration;
            double magnitude = Math.Sqrt(data.X * data.X + data.Y * data.Y + data.Z * data.Z);
            double deviation = Math.Abs(magnitude - 1.0);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (deviation > Threshold)
                {
                    MotionDetected = true;

                    _clearTimer?.Stop();
                    _clearTimer?.Dispose();
                    _clearTimer = new System.Timers.Timer(1500);
                    _clearTimer.Elapsed += (_, _) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() => MotionDetected = false);
                        _clearTimer?.Stop();
                    };
                    _clearTimer.AutoReset = false;
                    _clearTimer.Start();
                }
            });
        }
    }
}