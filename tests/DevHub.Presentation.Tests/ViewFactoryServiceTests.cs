using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;
using DevHub.Presentation.Registry;

namespace DevHub.Presentation.Tests;

public class ViewFactoryServiceTests
{
    [Fact]
    public void RegisterAll_ShouldRegisterSingletonViewModels()
    {
        var registry = new ViewRegistry();
        var services = new ServiceCollection();
        var factory = new ViewFactoryService(registry, null!);

        factory.RegisterAll(services, typeof(TestSingletonViewModel).Assembly);

        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestSingletonViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void RegisterAll_ShouldRegisterTransientViewModels()
    {
        var registry = new ViewRegistry();
        var services = new ServiceCollection();
        var factory = new ViewFactoryService(registry, null!);

        factory.RegisterAll(services, typeof(TestTransientViewModel).Assembly);

        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestTransientViewModel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void GetView_ShouldThrow_WhenNotRegistered()
    {
        var registry = new ViewRegistry();
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var factory = new ViewFactoryService(registry, provider);

        Assert.Throws<InvalidOperationException>(() => factory.GetView<TestSingletonViewModel>());
    }
}

[SingletonViewModel]
public class TestSingletonViewModel : ViewModelBase { }

public class TestTransientViewModel : ViewModelBase { }
