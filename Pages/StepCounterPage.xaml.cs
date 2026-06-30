using StepsCounter.ViewModels;

namespace StepsCounter.Pages;

public partial class StepCounterPage : ContentPage
{
    public StepCounterPage(StepCounterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}