namespace DevHub.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NavigationViewAttribute : Attribute
{
    public string Key { get; }

    public NavigationViewAttribute(string key)
    {
        Key = key;
    }
}
