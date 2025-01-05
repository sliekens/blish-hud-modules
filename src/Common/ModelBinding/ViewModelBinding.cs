

using System.ComponentModel;
using System.Linq.Expressions;

namespace SL.Common.ModelBinding;

public abstract class ViewModelBinding<TViewModel, TData> : IDisposable where TViewModel : ViewModel
{
    public TViewModel ViewModel { get; }

    public string ViewModelPropertyName { get; }

    private Lazy<Func<TData>> ViewModelRead { get; }

    private Lazy<Action<TData>> ViewModelWrite { get; }

    protected ViewModelBinding(TViewModel viewModel, Expression<Func<TViewModel, TData>> propertySelector)
    {
        ViewModel = viewModel;
        ViewModelPropertyName = ((MemberExpression)propertySelector.Body).Member.Name;
        ViewModelRead = new Lazy<Func<TData>>(() =>
        {
            var compiled = propertySelector.Compile();
            return () => compiled(ViewModel);
        });
        ViewModelWrite = new Lazy<Action<TData>>(() =>
        {
            var memberExpression = (MemberExpression)propertySelector.Body;
            var property = typeof(TViewModel).GetProperty(memberExpression.Member.Name) ?? throw new InvalidOperationException();
            return value => property.SetValue(ViewModel, value);
        });
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == ViewModelPropertyName)
        {
            UpdateView(ViewModelRead.Value());
        }
    }

    protected TData Snapshot() => ViewModelRead.Value();

    protected abstract void UpdateView(TData data);

    protected void UpdateModel(TData data)
    {
        ViewModelWrite.Value(data);
    }

    protected virtual void Dispose(bool disposing)
    {
        ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}