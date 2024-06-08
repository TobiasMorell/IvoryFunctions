using System.Reflection;
using Functions.MassTransit.Tests.TestFunctions;
using IvoryFunctions.Abstractions;
using IvoryFunctions.Blob;
using IvoryFunctions.Blob.Implementations;
using IvoryFunctions.Helpers;
using IvoryFunctions.Http;
using IvoryFunctions.Http.Models;
using IvoryFunctions.Setup;

namespace Functions.MassTransit.Tests.Helpers;

public class TypeHelperTests
{
    [Fact]
    public void DiscoverFunctions_discovers_functions_in_given_types()
    {
        // Act
        var functions = TypeHelper.DiscoverFunctions(typeof(TestFunctions.TestFunctions));

        // Assert
        var exampleFunction = functions.FirstOrDefault(
            f => f.FunctionAttribute.Name == nameof(TestFunctions.TestFunctions.TestFunction)
        );
        exampleFunction.ShouldNotBeNull();
        exampleFunction.MessageType.ShouldBe(typeof(ExampleFunctionMessage));
        var qtf = exampleFunction.ShouldBeOfType<QueueTriggeredFunction>();
        qtf.QueueTriggerAttribute.QueueName.ShouldBe("example-function");
        qtf.IsSingleton.ShouldBeFalse();
        qtf.Method.ShouldBe(
            typeof(TestFunctions.TestFunctions).GetMethod(
                nameof(TestFunctions.TestFunctions.TestFunction)
            )!
        );
    }

    [Fact]
    public void DiscoverFunctions_sets_singleton_attribute_if_present_on_method()
    {
        // Act
        var functions = TypeHelper.DiscoverFunctions(typeof(TestFunctions.TestFunctions));

        // Assert
        var exampleFunction = functions.FirstOrDefault(
            f => f.FunctionAttribute.Name == nameof(TestFunctions.TestFunctions.SingletonFunction)
        );
        exampleFunction.ShouldNotBeNull();
        var qtf = exampleFunction.ShouldBeOfType<QueueTriggeredFunction>();
        qtf.IsSingleton.ShouldBeTrue();
    }

    [Fact]
    public void DiscoverFunctions_throws_InvalidOperationException_when_method_has_no_parameters()
    {
        // Act
        Action act = () => TypeHelper.DiscoverFunctions(typeof(FunctionWithoutParameters));

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void DiscoverFunctions_throws_InvalidOperationException_when_method_has_first_parameter_without_trigger_decoration()
    {
        // Act
        Action act = () => TypeHelper.DiscoverFunctions(typeof(FunctionWithoutTriggerDecoration));

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void DiscoverFunctions_discovers_timer_triggers()
    {
        // Act
        var functions = TypeHelper.DiscoverFunctions(typeof(FunctionWithTimerTrigger));

        // Assert
        var exampleFunction = functions.FirstOrDefault(
            f => f.FunctionAttribute.Name == nameof(FunctionWithTimerTrigger.ExampleTimerFunction)
        );
        exampleFunction.ShouldNotBeNull();
        exampleFunction.MessageType.ShouldBe(typeof(TimerInfo));
        var ttf = exampleFunction.ShouldBeOfType<TimerTriggeredFunction>();
        ttf.TimerTriggerAttribute.CronExpression.ShouldBe("0/1 * * * * ?");
        ttf.TimerTriggerAttribute.RunOnStartup.ShouldBeFalse();
    }

    [Fact]
    public void DiscoverFunctions_discovers_blob_triggers()
    {
        // Act
        var functions = TypeHelper.DiscoverFunctions(typeof(FunctionWithBlobTrigger));

        // Assert
        var exampleFunction = functions.FirstOrDefault(
            f => f.FunctionAttribute.Name == nameof(FunctionWithBlobTrigger.ExampleBlobFunction)
        );
        exampleFunction.ShouldNotBeNull();
        exampleFunction.MessageType.ShouldBe(typeof(byte[]));
        var ttf = exampleFunction.ShouldBeOfType<BlobTriggeredFunction>();
        ttf.BlobTriggerAttribute.BlobPath.ShouldBe("example-blob-container/{name}.txt");
    }

    [Fact]
    public void DiscoverFunctions_discovers_http_triggers()
    {
        // Act
        var functions = TypeHelper.DiscoverFunctions(typeof(FunctionWithHttpTrigger));

        // Assert
        var exampleFunction = functions.FirstOrDefault(
            f => f.FunctionAttribute.Name == nameof(FunctionWithHttpTrigger.HttpFunction)
        );
        exampleFunction.ShouldNotBeNull();
        exampleFunction.MessageType.ShouldBe(typeof(HttpRequestData));
        var ttf = exampleFunction.ShouldBeOfType<HttpTriggeredFunction>();
        ttf.HttpTriggerAttribute.Route.ShouldBe("example-http-function");
    }

    [Fact]
    public void DiscoverFunctionRegistrators_discovers_registrators_in_referenced_assemblies()
    {
        // Act
        var registrators = TypeHelper.DiscoverFunctionRegistrators(Assembly.GetExecutingAssembly());

        // Assert
        registrators.ShouldNotBeEmpty();

        var queues = registrators.GetValueOrDefault(typeof(QueueTriggeredFunction));
        queues.ShouldNotBeNull().ShouldBe(typeof(QueueFunctionsRegistrator));

        var timers = registrators.GetValueOrDefault(typeof(TimerTriggeredFunction));
        timers.ShouldNotBeNull().ShouldBe(typeof(TimerFunctionsRegistrator));

        var blobs = registrators.GetValueOrDefault(typeof(BlobTriggeredFunction));
        blobs.ShouldNotBeNull().ShouldBe(typeof(BlobFunctionsRegistrator));
    }
}
