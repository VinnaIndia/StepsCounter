namespace StepsCounter.Services;

public interface IStepCounterService
{
    event Action<int>? StepsChanged;
    bool IsSupported();
    void Start();
    void Stop();
    void ResetSteps();  // add this
}