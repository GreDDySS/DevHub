using System.Windows;
using System.Windows.Input;

namespace DevHub.Presentation.Attached;

public static class MouseBehavior
{
    public static readonly DependencyProperty LeftButtonUpCommandProperty =
        DependencyProperty.RegisterAttached(
            "LeftButtonUpCommand",
            typeof(ICommand),
            typeof(MouseBehavior),
            new PropertyMetadata(null, OnLeftButtonUpCommandChanged));

    public static ICommand GetLeftButtonUpCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(LeftButtonUpCommandProperty);

    public static void SetLeftButtonUpCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(LeftButtonUpCommandProperty, value);

    public static readonly DependencyProperty LeftButtonUpCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "LeftButtonUpCommandParameter",
            typeof(object),
            typeof(MouseBehavior),
            new PropertyMetadata(null));

    public static object GetLeftButtonUpCommandParameter(DependencyObject obj)
        => obj.GetValue(LeftButtonUpCommandParameterProperty);

    public static void SetLeftButtonUpCommandParameter(DependencyObject obj, object value)
        => obj.SetValue(LeftButtonUpCommandParameterProperty, value);

    private static void OnLeftButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            if (e.NewValue is ICommand)
                element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is DependencyObject d)
        {
            var command = GetLeftButtonUpCommand(d);
            var parameter = GetLeftButtonUpCommandParameter(d) ?? d.GetValue(FrameworkElement.DataContextProperty);

            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }

    public static readonly DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.RegisterAttached(
            "DoubleClickCommand",
            typeof(ICommand),
            typeof(MouseBehavior),
            new PropertyMetadata(null, OnDoubleClickCommandChanged));

    public static ICommand GetDoubleClickCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(DoubleClickCommandProperty);

    public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(DoubleClickCommandProperty, value);

    public static readonly DependencyProperty DoubleClickCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "DoubleClickCommandParameter",
            typeof(object),
            typeof(MouseBehavior),
            new PropertyMetadata(null));

    public static object GetDoubleClickCommandParameter(DependencyObject obj)
        => obj.GetValue(DoubleClickCommandParameterProperty);

    public static void SetDoubleClickCommandParameter(DependencyObject obj, object value)
        => obj.SetValue(DoubleClickCommandParameterProperty, value);

    private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            if (e.NewValue is ICommand)
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is DependencyObject d)
        {
            var command = GetDoubleClickCommand(d);
            var parameter = GetDoubleClickCommandParameter(d);

            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }
}
