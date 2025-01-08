using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class TextBoxBinding<TViewModel> : ViewModelBinding<TViewModel, string>
    where TViewModel : ViewModel
{
    public TextBox TextBox { get; }

    public TextBoxBinding(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, TextBox textBox) : base(viewModel, propertySelector)
    {
        TextBox = textBox;
        textBox.Text = Snapshot();
        textBox.TextChanged += TextChanged;
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