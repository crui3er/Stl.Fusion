using System;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;

namespace Stl.Fusion
{
    public class RegisterComputeServiceAttribute : RegisterServiceAttribute
    {
        public RegisterComputeServiceAttribute(Type? serviceType = null) : base(serviceType) { }

        public override void Register(IServiceCollection services, Type implementationType)
            => services.AddFusion().AddComputeService(
                ServiceType ?? implementationType, implementationType, Lifetime);
    }
}