using System.Linq.Expressions;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class NumberInputBinding<TViewModel> : ViewModelBinding<TViewModel, int>
    where TViewModel : ViewModel
{
    public NumberInput NumberInput { get; }

    public NumberInputBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, int>> propertySelector,
        NumberInput numberInput,
        BindingMode bindingMode
    ) : base(viewModel, propertySelector, bindingMode)
    {
        ThrowHelper.ThrowIfNull(numberInput);
        NumberInput = numberInput;

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            UpdateView(Snapshot());
        }

        if (bindingMode is BindingMode.ToModel or BindingMode.Bidirectional)
        {
            numberInput.ValueChanged += ValueChanged;
        }
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
