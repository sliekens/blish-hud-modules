using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace SL.Common.ModelBinding;

public abstract class ViewModelBinding<TViewModel, TData> : IDisposable where TViewModel : ViewModel
{
    public TViewModel ViewModel { get; }
    public string ViewModelPropertyName { get; }
    private Lazy<Func<TData>> ViewModelRead { get; }
    private Lazy<Action<TData>> ViewModelWrite { get; }

    protected ViewModelBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, TData>> viewModelPropertySelector,
        BindingMode bindingMode
    )
    {
        ViewModel = viewModel;
        ViewModelPropertyName = ((MemberExpression)viewModelPropertySelector.Body).Member.Name;
        ViewModelRead = new Lazy<Func<TData>>(() =>
        {
            Func<TViewModel, TData> compiled = viewModelPropertySelector.Compile();
            return () => compiled(ViewModel);
        });
        ViewModelWrite = new Lazy<Action<TData>>(() =>
        {
            PropertyInfo propertyInfo = (PropertyInfo)((MemberExpression)viewModelPropertySelector.Body).Member;
            return value => propertyInfo.SetValue(viewModel, value);
        });

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == ViewModelPropertyName)
        {
            UpdateView(Snapshot());
        }
    }

    protected TData Snapshot()
    {
        return ViewModelRead.Value();
    }

    protected abstract void UpdateView(TData data);

    protected void UpdateModel(TData data)
    {
        ViewModelWrite.Value(data);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
