using System.ComponentModel;
using System.Globalization;
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
        ThrowHelper.ThrowIfNull(viewModel);
        ThrowHelper.ThrowIfNull(viewModelPropertySelector);
        ViewModel = viewModel;
        ViewModelPropertyName = ExtractMemberInfo(viewModelPropertySelector).Name;
        ViewModelRead = new Lazy<Func<TData>>(() =>
        {
            Func<TViewModel, TData> compiled = viewModelPropertySelector.Compile();
            return () => compiled(ViewModel);
        });
        ViewModelWrite = new Lazy<Action<TData>>(() =>
        {
            PropertyInfo propertyInfo = (PropertyInfo)ExtractMemberInfo(viewModelPropertySelector);
            return value =>
            {
                if (propertyInfo.PropertyType == typeof(TData))
                {
                    propertyInfo.SetValue(viewModel, value);
                }
                else
                {
                    propertyInfo.SetValue(viewModel, Convert.ChangeType(value, propertyInfo.PropertyType, CultureInfo.CurrentCulture));
                }
            };
        });

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    protected static MemberInfo ExtractMemberInfo<T, TReturn>(Expression<Func<T, TReturn>> propertySelector)
    {
        ThrowHelper.ThrowIfNull(propertySelector);
        Expression expr = propertySelector.Body;
        while (expr is UnaryExpression unary)
        {
            expr = unary.Operand;
        }

        return ((MemberExpression)expr).Member;
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
