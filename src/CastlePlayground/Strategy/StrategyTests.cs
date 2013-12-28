using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace CastleExplorations
{
    public class StrategyTests
    {
        readonly IWindsorContainer container = new WindsorContainer();
        
        public StrategyTests()
        {
            container.Register(
                Component.For<ISpecificService>().ImplementedBy<SpecifcServiceDecorator>().LifestyleTransient(),
                Component.For<ISpecificService>().ImplementedBy<SpecificServiceClientOne>().LifestyleTransient(),
                Component.For<ISpecificService>().ImplementedBy<SpecificServiceClientTwo>().LifestyleTransient(),

                Component.For<IContext>().UsingFactoryMethod(()=>Context.CurrentContext).LifestyleTransient()
                );
            container.Kernel.AddHandlersFilter(new HandlerFilter());

            Context.CurrentContext = new Context { Name = "ClientOne" };
        }

        [Fact]
        public void CanRegisterComponentUsingFactoryMethod()
        {
            var ctx = container.Resolve<IContext>();
            Assert.NotNull(ctx);

        }


        [Fact]
        public void CanResolveDecorator()
        {
            var svc = container.Resolve<ISpecificService>();

            Assert.NotNull(svc);
            Assert.IsType<SpecifcServiceDecorator>(svc);
        }

       [Fact]
        public void CanResolveDecoratedServiceForClientOne()
        {
            var svc = container.Resolve<ISpecificService>();

            var ds = svc as SpecifcServiceDecorator;
            Assert.NotNull(ds);
            Assert.IsType<SpecificServiceClientOne>(ds.wrappedSvc);
        }

        [Fact]
        public void CanResolveDecoratedServiceForClientTwo()
        {
            Context.CurrentContext = new Context {Name = "ClientTwo"};
            var svc = container.Resolve<ISpecificService>();

            var ds = svc as SpecifcServiceDecorator;
            Assert.NotNull(ds);
            Assert.IsType<SpecificServiceClientTwo>(ds.wrappedSvc);
        }

        
    }
}