using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace SL.Common.ModelBinding;

public sealed class GenericBinding<TViewModel, TControl, TData> : ViewModelBinding<TViewModel, TData>
    where TViewModel : ViewModel
    where TControl : INotifyPropertyChanged
{
    public TControl Control { get; }

    public string ControlPropertyName { get; }

    private Lazy<Func<TData>> ControlRead { get; }

    private Lazy<Action<TData>> ControlWrite { get; }

    public GenericBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, TData>> viewModelPropertySelector,
        TControl control,
        Expression<Func<TControl, TData>> controlPropertySelector,
        BindingMode bindingMode
    ) : base(viewModel, viewModelPropertySelector, bindingMode)
    {
        ThrowHelper.ThrowIfNull(control);
        ThrowHelper.ThrowIfNull(controlPropertySelector);
        Control = control;
        ControlPropertyName = ((MemberExpression)controlPropertySelector.Body).Member.Name;
        ControlRead = new Lazy<Func<TData>>(() =>
        {
            Func<TControl, TData> compiled = controlPropertySelector.Compile();
            return () => compiled(control);
        });
        ControlWrite = new Lazy<Action<TData>>(() =>
        {
            PropertyInfo propertyInfo = (PropertyInfo)((MemberExpression)controlPropertySelector.Body).Member;
            return value => propertyInfo.SetValue(control, value);
        });

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            UpdateView(Snapshot());
        }

        if (bindingMode is BindingMode.ToModel or BindingMode.Bidirectional)
        {
            Control.PropertyChanged += ControlOnPropertyChanged;
        }
    }

    private void ControlOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateModel(ControlRead.Value());
    }

    protected override void UpdateView(TData data)
    {
        ControlWrite.Value(data);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Control.PropertyChanged -= ControlOnPropertyChanged;
        }

        base.Dispose(disposing);
    }
}
