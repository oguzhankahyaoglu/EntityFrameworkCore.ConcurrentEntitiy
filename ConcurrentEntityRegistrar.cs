using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.ConcurrentEntitiy
{
    public static class ConcurrentEntityRegistrar
    {
        internal static bool IsModelBuilderInitialized;
        internal static bool IsInterceptorRegistered;

        public static void InitializeModelBuilder(ModelBuilder modelBuilder)
        {
            ConcurrentEntityInterceptor.InitializeModelBuilder(modelBuilder);
            IsModelBuilderInitialized = true;
        }

        public static void RegisterInterceptor(DbContextOptionsBuilder builder,
            ILogger logger,
            Func<Exception> concurrencyExceptionFactory)
        {
            builder.AddInterceptors(new ConcurrentEntityInterceptor(logger, concurrencyExceptionFactory));
            IsInterceptorRegistered = true;
        }
    }
}