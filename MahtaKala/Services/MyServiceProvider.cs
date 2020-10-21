using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public static class MyServiceProvider
    {
        static IServiceProvider serviceProvider;
        public static void SetSetviceProvider(IServiceProvider serviceProvider)
        {
            MyServiceProvider.serviceProvider = serviceProvider;
        }

        public static TService ResolveService<TService>()
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }

    }
}
