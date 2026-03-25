namespace DevHub.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ViewForAttribute : Attribute
{
    public Type ViewModelType { get; }

    public ViewForAttribute(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}
