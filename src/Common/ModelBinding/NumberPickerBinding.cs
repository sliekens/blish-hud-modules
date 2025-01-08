using System.Linq.Expressions;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class NumberPickerBinding<TViewModel> : ViewModelBinding<TViewModel, int>
    where TViewModel : ViewModel
{
    public NumberPicker NumberPicker { get; }

    public NumberPickerBinding(TViewModel viewModel, Expression<Func<TViewModel, int>> propertySelector, NumberPicker numberPicker) : base(viewModel, propertySelector)
    {
        NumberPicker = numberPicker;
        numberPicker.Value = Snapshot();
        numberPicker.ValueChanged += ValueChanged;
    }

    private void ValueChanged(object sender, EventArgs e)
    {
        UpdateModel(NumberPicker.Value);
    }

    protected override void UpdateView(int data)
    {
        NumberPicker.Value = data;
    }

    protected override void Dispose(bool disposing)
    {
        NumberPicker.ValueChanged -= ValueChanged;
        base.Dispose(disposing);
    }
}