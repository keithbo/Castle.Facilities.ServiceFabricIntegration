# Castle.Facilities.ServiceFabricIntegration
Castle Windsor integration support for Azure ServiceFabric

This project provides a Castle Windsor facility for injecting Azure ServiceFabric reliable services and actors.
There are two available nuget packages:
* [Castle.Facilities.ServiceFabricIntegration](https://www.nuget.org/packages/Castle.Facilities.ServiceFabricIntegration/)
* [Castle.Facilities.ServiceFabricIntegration.Actors](https://www.nuget.org/packages/Castle.Facilities.ServiceFabricIntegration.Actors/)

As you might guess the first provides the basic facility as well as automatically loaded modules for Stateful and Stateless services. The Actors package adds an additional module, and dependencies, for integrating Azure ServiceFabric Actors.

## The Basics
For starters the ServiceFabricIntegration is designed to work with the Castle Windsor component model via facilities. As such an Installer is generally the best place to bootstrap ServiceFabricIntegration as shown below:
```
using Castle.Facilities.ServiceFabricIntegration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

public class Installer : IWindsorInstaller
{
  public void Install(IWindsorContainer container, IConfigurationStore store)
  {
    container.AddFacility<ServiceFabricFacility>();
    
    // Insert Your Registrations Here
  }
}
```

This is effectively all that is needed to bootstrap into the ServiceFabric runtime registration process. Well, that and actually registering your service classes. ServiceFabricIntegration abstracts out any need to explicitly call `ServiceRuntime.RegisterServiceAsync` or `ActorRuntime.RegisterActorAsync<TActor>` registration methods.

## A simple modular design
ServiceFabricIntegration is designed around a modular abstraction over the Castle Windsor component model. This allows for new registration handling to be added as needed, for instance Actors are provided with a module from a dependant NuGet package.
Modules must all implement `Castle.Facilities.ServiceFabricIntegration.IServiceFabricModule` interface.
IServiceFabricModule has 4 methods:
  1. Init
  2. Contribute
  3. CanRegister
  4. RegisterComponent

Each of these methods relates to different phases of the CastleWindsor registration process. (TBD: Needs more detail)

## Facility Configuration
As listed in The Basics, facility registration is simple. However ServiceFabricIntegration has the ability to add modules. Additional modules beyond Stateful and Stateless need to be added at facility registration time through `ServiceFabricFacility.Configure(Action<IServiceFabricFacilityConfigurer>)`.

The general pattern to use is:
```
container.AddFacility<ServiceFabricFacility>(facility =>
  facility.Configure(config =>
    config.Using(new MyModule1(), new MyModule2(), ...)));
```

Actors has a shorthand extension method `facility.Configure(config => config.UsingActors())`.

## Reliable Services
Because Reliable Services are the foundation of Azure ServiceFabric they are supported by default and ServiceFabricFacility imports modules for both Stateful and Stateless services automatically.

Multiple component model extension methods are available to trigger a component registration for inclusion as services.
  1. `ComponentRegistration<TService>.AsStatefulService(string, Action<IStatefulConfigurer> configure = null)`
```
  container.Register(
    Component.For<MyStatefulService>()
             .AsStatefulService("MyStatefulServiceType", config =>
             {
               config.StateManagerConfiguration = new ReliableStateManagerConfiguration(...);
             }));
```
Stateful service registration takes an optional Action delegate which can be used to specify initialization-time ReliableStateManager configuration settings.

  2. `ComponentRegistration<TService>.AsStatelessService(string)`
```
  container.Register(
    Component.For<MyStatelessService>()
             .AsStatelessService("MyStatelessServiceType"));
```

As shown the string passed in to each of these methods is the Type of your service, which must match what is declared in your ServiceManifest.xml.

```
  <ServiceTypes>
    <StatefulServiceType ServiceTypeName="MyStatefulServiceType" />
    <StatelessServiceType ServiceTypeName="MyStatelessServiceType" />
  </ServiceTypes>
```

## Reliable Actors
The other provided support is for Azure ServiceFabric Actors. This is a seperate package because there are different dependencies necessary to support Actors even though they require Reliable Services themselves.

Adding actors requires a slight modification to the facility registration from earlier:
```
...
  container.AddFacility<ServiceFabricFacility>(facility => facility.Configure(config => config.UsingActors()));
...
```

And registration of actor types is very similar to services:
```
  container.Register(
    Component.For<MyActor>()
             .AsActor()
             .LifestyleTransient());
```

As you will note there are `AsActor()` extension methods that handles all inclusion into ServiceFabric. However you will also note the actor is declared as Transient. This is important to note because CastleWindsor registers as Singleton by default, and a singleton Actor is not very useful.

Another thing to note that is not shown here is Actors deactivate after a set time limit which can affect lifetime. To address this ServiceFabricIntegration uses a interceptor (ActorDeactivationInterceptor) to capture the `OnDeactivateAsync` call ServiceFabric makes and it releases the actor from CastleWindsor after the call finishes. This isn't overridable, and the only way to affect lifetime is using a custom lifetime in CastleWindsor.

### Customizing Actor Initialization
By default `AsActor()` will use [ActorService](https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.actors.runtime.actorservice) as the Reliable Actors service class. There are however a number of ways to extend the actor registration process.
__Note:__ ActorService is registered by the ActorModule by default, but any custom service type needs to be explicitly registered before the Actor registration in order to be used.

  1. `ComponentRegistration<TActor>.AsActor<TActor, TService>()`
This extension variant overrides the registered ActorService with type `TService`.

  2. `ComponentRegistration<TActor>.AsActor(Action<IActorConfigurer> configure = null)`
```
Component.For<MyActor>()
         .AsActor(config =>
         {
           config.WithService<TService>();
           config.WithService(typeof(TService));
           config.ServiceType = typeof(TService);
           
           config.ServiceSettings = new ActorServiceSettings();
           config.StateManagerFactory = [Custom Func<ActorBase, IActorStateProvider, IActorStateManager>];
         })
```
This is the standard AsActor() extension method, and it takes an optional `Action<IActorConfigurer>` to configure custom initialization values for the ActorService associated with the registered Actor.

## ETW EventSource
ServiceFabricIntegration currently makes use of EventSource as its primary way to report operation and errors.

Namespaces:
Castle.Facilities.ServiceFabricIntegration
Castle.Facilities.ServiceFabricIntegration-Actors
