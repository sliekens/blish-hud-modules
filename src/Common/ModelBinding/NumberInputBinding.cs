using System.Linq.Expressions;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class NumberInputBinding<TViewModel> : ViewModelBinding<TViewModel, int>
    where TViewModel : ViewModel
{
    public NumberInput NumberInput { get; }

    public NumberInputBinding(TViewModel viewModel, Expression<Func<TViewModel, int>> propertySelector, NumberInput numberInput) : base(viewModel, propertySelector)
    {
        NumberInput = numberInput;
        numberInput.Value = Snapshot();
        numberInput.ValueChanged += ValueChanged;
    }

    private void ValueChanged(object sender, EventArgs e)
    {
        UpdateModel(NumberInput.Value);
    }

    protected override void UpdateView(int data)
    {
        NumberInput.Value = data;
    }

    protected override void Dispose(bool disposing)
    {
        NumberInput.ValueChanged -= ValueChanged;
        base.Dispose(disposing);
    }
}