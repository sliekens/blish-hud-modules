using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class TextBoxBinding<TViewModel> : ViewModelBinding<TViewModel, string>
    where TViewModel : ViewModel
{
    public TextBox TextBox { get; }

    public TextBoxBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, string>> propertySelector,
        TextBox textBox,
        BindingMode bindingMode
    ) : base(viewModel, propertySelector, bindingMode)
    {
        TextBox = textBox;

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            UpdateView(Snapshot());
        }

        if (bindingMode is BindingMode.ToModel or BindingMode.Bidirectional)
        {
            textBox.TextChanged += TextChanged;
        }
    }

    private void TextChanged(object sender, EventArgs e)
    {
        UpdateModel(TextBox.Text);
    }

    protected override void UpdateView(string data)
    {
        TextBox.Text = data;
    }

    protected override void Dispose(bool disposing)
    {
        TextBox.TextChanged -= TextChanged;
        base.Dispose(disposing);
    }
}
