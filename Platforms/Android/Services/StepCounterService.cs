#if ANDROID

using Android.Content;
using Android.Hardware;
using Microsoft.Maui.Storage;
using StepsCounter.Services;

namespace StepsCounter.Platforms.Android.Services;

public class StepCounterService :
    Java.Lang.Object,
    IStepCounterService,
    ISensorEventListener
{
    private readonly SensorManager _sensorManager;
    private readonly Sensor? _stepSensor;

    private float _initialValue = -1;

    public event Action<int>? StepsChanged;

    public StepCounterService()
    {
        _sensorManager =
            (SensorManager)global::Android.App.Application.Context
            .GetSystemService(Context.SensorService)!;

        _stepSensor =
            _sensorManager.GetDefaultSensor(SensorType.StepCounter);

        if (_stepSensor != null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"FOUND STEP SENSOR: {_stepSensor.Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(
                "STEP SENSOR IS NULL");
        }

        _initialValue =
            Preferences.Get("initial_steps", -1f);
    }

    public bool IsSupported()
    {
        bool supported = _stepSensor != null;

        System.Diagnostics.Debug.WriteLine(
            $"IsSupported = {supported}");

        return supported;
    }

    public void Start()
    {
        if (_stepSensor == null)
        {
            System.Diagnostics.Debug.WriteLine(
                "Cannot start. Step sensor is null.");
            return;
        }

        bool registered = _sensorManager.RegisterListener(
      this,
      _stepSensor,
      SensorDelay.Fastest);  // was SensorDelay.Normal

        System.Diagnostics.Debug.WriteLine(
            $"REGISTER LISTENER RESULT: {registered}");
    }

    public void Stop()
    {
        _sensorManager.UnregisterListener(this);

        System.Diagnostics.Debug.WriteLine(
            "STEP COUNTER STOPPED");
    }
    public void ResetSteps()
    {
        _initialValue = -1;
        Preferences.Remove("initial_steps");
        System.Diagnostics.Debug.WriteLine("STEPS RESET");
    }
    public void OnSensorChanged(SensorEvent? e)
    {
        if (e == null)
            return;

        float totalSteps = e.Values[0];

        System.Diagnostics.Debug.WriteLine(
            $"STEP EVENT RECEIVED: {totalSteps}");

        if (_initialValue < 0)
        {
            _initialValue = totalSteps;

            Preferences.Set(
                "initial_steps",
                _initialValue);

            System.Diagnostics.Debug.WriteLine(
                $"INITIAL VALUE SET: {_initialValue}");
        }

        int currentSteps =
            (int)(totalSteps - _initialValue);

        System.Diagnostics.Debug.WriteLine(
            $"CURRENT STEPS: {currentSteps}");

        StepsChanged?.Invoke(currentSteps);
    }

    public void OnAccuracyChanged(
        Sensor? sensor,
        SensorStatus accuracy)
    {
        System.Diagnostics.Debug.WriteLine(
            $"ACCURACY CHANGED: {accuracy}");
    }
}

#endif