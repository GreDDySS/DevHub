using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevHub.Domain.Models;

public abstract class BaseModel : INotifyPropertyChanged, IEquatable<BaseModel>
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private Guid _id = Guid.NewGuid();

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private DateTime _createdAt = DateTime.UtcNow;

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    private DateTime _updatedAt = DateTime.UtcNow;

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => SetProperty(ref _updatedAt, value);
    }

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool Equals(BaseModel? other)
        => other is not null && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode();

    public override bool Equals(object? obj)
        => obj is BaseModel other && Equals(other);
}
