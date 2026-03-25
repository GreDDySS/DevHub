using DevHub.Presentation.Registry;

namespace DevHub.Presentation.Tests;

public class ViewRegistryTests
{
    [Fact]
    public void Register_ShouldStoreRegistration()
    {
        var registry = new ViewRegistry();
        var registration = new ViewRegistration
        {
            ViewType = typeof(object),
            ViewModelType = typeof(string),
            IsNavigation = false,
            IsSingleton = false
        };

        registry.Register(registration);

        var result = registry.GetByViewModel(typeof(string));
        Assert.NotNull(result);
        Assert.Equal(typeof(object), result.ViewType);
        Assert.Equal(typeof(string), result.ViewModelType);
    }

    [Fact]
    public void Register_ShouldStoreNavigationKey()
    {
        var registry = new ViewRegistry();
        var registration = new ViewRegistration
        {
            ViewType = typeof(object),
            ViewModelType = typeof(string),
            IsNavigation = true,
            NavigationKey = "projects",
            IsSingleton = false
        };

        registry.Register(registration);

        var result = registry.GetByKey("projects");
        Assert.NotNull(result);
        Assert.Equal("projects", result.NavigationKey);
    }

    [Fact]
    public void GetByKey_ShouldReturnNull_WhenNotRegistered()
    {
        var registry = new ViewRegistry();

        var result = registry.GetByKey("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void GetByViewModel_ShouldReturnNull_WhenNotRegistered()
    {
        var registry = new ViewRegistry();

        var result = registry.GetByViewModel(typeof(string));

        Assert.Null(result);
    }

    [Fact]
    public void GetAllNavigationViews_ShouldReturnOnlyNavigationViews()
    {
        var registry = new ViewRegistry();

        registry.Register(new ViewRegistration
        {
            ViewType = typeof(object),
            ViewModelType = typeof(string),
            IsNavigation = true,
            NavigationKey = "projects",
            IsSingleton = false
        });

        registry.Register(new ViewRegistration
        {
            ViewType = typeof(int),
            ViewModelType = typeof(double),
            IsNavigation = false,
            IsSingleton = false
        });

        var navViews = registry.GetAllNavigationViews();

        Assert.Single(navViews);
        Assert.True(navViews.ContainsKey("projects"));
    }

    [Fact]
    public void Register_ShouldOverwrite_WhenSameViewModelRegistered()
    {
        var registry = new ViewRegistry();

        registry.Register(new ViewRegistration
        {
            ViewType = typeof(object),
            ViewModelType = typeof(string),
            IsNavigation = false,
            IsSingleton = false
        });

        registry.Register(new ViewRegistration
        {
            ViewType = typeof(int),
            ViewModelType = typeof(string),
            IsNavigation = false,
            IsSingleton = false
        });

        var result = registry.GetByViewModel(typeof(string));
        Assert.NotNull(result);
        Assert.Equal(typeof(int), result.ViewType);
    }
}
