using System.Windows.Input;
using StepsCounter.Services;

namespace StepsCounter.ViewModels;

public class StepCounterViewModel : BaseViewModel
{
    private readonly IStepCounterService _stepCounterService;

    private int _stepCount;
    private bool _isTracking;
    private string _statusMessage = "Tap Start";

    public int StepCount
    {
        get => _stepCount;
        set
        {
            _stepCount = value;
            OnPropertyChanged();
        }
    }

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
    private string _statusColor = "Green";
    public string StatusColor
    {
        get => _statusColor;
        set
        {
            _statusColor = value;
            OnPropertyChanged();
        }
    }

    private bool _isNotSupported;
    public bool IsNotSupported
    {
        get => _isNotSupported;
        set
        {
            _isNotSupported = value;
            OnPropertyChanged();
        }
    }
    public bool CanStart => !IsTracking;

    public bool CanStop => IsTracking;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand ResetCommand { get; }

    public StepCounterViewModel(
        IStepCounterService stepCounterService)
    {
        _stepCounterService = stepCounterService;

        _stepCounterService.StepsChanged += steps =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StepCount = steps;
            });
        };

        StartCommand = new Command(async () => await StartTracking());
        StopCommand = new Command(StopTracking);
        ResetCommand = new Command(Reset);
    }

    private async Task StartTracking()
    {
        var permission =
              await Permissions.CheckStatusAsync<ActivityRecognitionPermission>();


        if (permission != PermissionStatus.Granted)
        {
            permission =
               await Permissions.RequestAsync<ActivityRecognitionPermission>();
        }

        if (permission != PermissionStatus.Granted)
        {
            StatusMessage = "Permission denied";
            return;
        }

        if (!_stepCounterService.IsSupported())
        {
            StatusMessage = "Step Counter Sensor Not Available";
            return;
        }

        _stepCounterService.Start();

        IsTracking = true;
        StatusMessage = "Tracking Steps...";
    }

    private void StopTracking()
    {
        _stepCounterService.Stop();

        IsTracking = false;
        StatusMessage = "Stopped";
    }

    private void Reset()
    {
        _stepCounterService.ResetSteps();
        StepCount = 0;
        StatusMessage = "Reset Completed";
    }
}