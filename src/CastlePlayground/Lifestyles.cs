using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace CastleExplorations
{
    public interface IController : IDisposable
    {
        int DoSomething();

        bool Disposed { get; set; }
    }

    public interface IController2
    {
        int DoSomething();
    }

    public class Controller2:IController2
    {
        public int DoSomething()
        {
            return 43;
        }
    }

    public class Controller : IController
    {
        // notice it's not virtual!
        public void Dispose()
        {
            // some clean up logic here
            Disposed = true;
        }

        public int DoSomething()
        {
            return 42;
        }

        public bool Disposed
        {
            get;
            set;
        }
    }

    public class Lifestyles
    {
        IWindsorContainer container = new WindsorContainer();
        public Lifestyles()
        {
            container.Register(
                Component.For<IController>().ImplementedBy<Controller>().LifestyleTransient(),
                Component.For<IController2>().ImplementedBy<Controller2>().LifestyleTransient()
                );
        }

        [Fact]
        public void WindsotTracksDisposableComponents()
        {
            var c = container.Resolve<IController>();
            Assert.True(container.Kernel.ReleasePolicy.HasTrack(c));
            container.Release(c);
            Assert.False(container.Kernel.ReleasePolicy.HasTrack(c));
        }

        [Fact]
        public void WinsorDoesntTrackNonDispasableComponents()
        {
            var c = container.Resolve<IController2>();
            Assert.False(container.Kernel.ReleasePolicy.HasTrack(c));
        }
    }
}