using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.ConcurrentEntitiy
{
    public static class ConcurrentEntityRegistrar
    {
        public static void InitializeModelBuilder(ModelBuilder modelBuilder)
        {
            ConcurrentEntityInterceptor.InitializeModelBuilder(modelBuilder);
        }

        public static void RegisterInterceptor(DbContextOptionsBuilder builder, 
            ILogger logger,
            Func<Exception> concurrencyExceptionFactory)
        {
            builder.AddInterceptors(new ConcurrentEntityInterceptor(logger,concurrencyExceptionFactory));
        }
    }
}