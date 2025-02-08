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
        TControl control
,
        Expression<Func<TControl, TData>> controlPropertySelector)
        : base(viewModel, viewModelPropertySelector)
    {
        Control = control;
        ControlPropertyName = ((MemberExpression)controlPropertySelector.Body).Member.Name;
        ControlRead = new Lazy<Func<TData>>(() =>
        {
            var compiled = controlPropertySelector.Compile();
            return () => compiled(control);
        });
        ControlWrite = new Lazy<Action<TData>>(() =>
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)controlPropertySelector.Body).Member;
            return value => propertyInfo.SetValue(control, value);
        });

        Control.PropertyChanged += ControlOnPropertyChanged;
        UpdateView(Snapshot());
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