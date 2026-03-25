using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Domain.Models;

public abstract class BaseModel : ObservableObject, IEquatable<BaseModel>
{
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
        OnPropertyChanged(nameof(UpdatedAt));
    }

    public bool Equals(BaseModel? other)
        => other is not null && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode();

    public override bool Equals(object? obj)
        => obj is BaseModel other && Equals(other);
}
