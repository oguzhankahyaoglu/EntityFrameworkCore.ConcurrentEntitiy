using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.ConcurrentEntitiy
{
    public class ConcurrentEntityRegistrar
    {
        internal static void InitializeModelBuilder(ModelBuilder modelBuilder)
        {
            ConcurrentEntityInterceptor.InitializeModelBuilder(modelBuilder);
        }

        internal static void RegisterInterceptor(DbContextOptionsBuilder builder, ILogger logger)
        {
            builder.AddInterceptors(new ConcurrentEntityInterceptor(logger));
        }
    }
}