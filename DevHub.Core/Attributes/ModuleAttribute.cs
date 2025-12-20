namespace DevHub.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        public string Name { get; }
        public bool IsSystem { get; }

        public ModuleAttribute(string name, bool isSystem = false)
        {
            Name = name;
            IsSystem = isSystem;
        }
    }
}
