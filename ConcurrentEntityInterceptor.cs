using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.ConcurrentEntitiy
{
    /// <summary>
    /// DB'deki ConcurrencyToken kolonunun default olarak versiyonla yönetilmesini sağlar.
    /// </summary>
    internal class ConcurrentEntityInterceptor : SaveChangesInterceptor
    {
        private static bool _isInitialized;
        private readonly ILogger _logger;
        private readonly Func<Exception> _concurrencyExceptionFactory;

        public ConcurrentEntityInterceptor(ILogger logger, Func<Exception> concurrencyExceptionFactory)
        {
            _logger = logger;
            _concurrencyExceptionFactory = concurrencyExceptionFactory;
        }

        internal static void InitializeModelBuilder(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!entityType.ClrType.IsAssignableTo(typeof(IConcurrentEntity)))
                    continue;

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IConcurrentEntity.ConcurrencyToken))
                    .IsConcurrencyToken()
                    ;

                modelBuilder.Entity(entityType.ClrType)
                    .Property<byte[]>("RowVersion")
                    .IsRowVersion()
                    ;
            }

            _isInitialized = true;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
                throw new ArgumentException(
                    "ConcurrentEntityInterceptor.InitializeModelBuilder metodu üzerinden dbcontext model builder'ı initialize edilmeli ki db değişikliklerini yansıtabilelim. OnModelCreating'de bu metodu çağırmak gereklidir!");

            var defaultValue = (long?) 1;

            var addedEntities = eventData.Context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Where(x => x.Entity is IConcurrentEntity)
                .ToList();
            foreach (var entry in addedEntities)
            {
                if (entry.Entity is not IConcurrentEntity)
                    continue;

                var token = entry.Property(nameof(IConcurrentEntity.ConcurrencyToken));
                token.CurrentValue = defaultValue;
            }

            var modifiedEntities = eventData.Context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Where(x => x.Entity is IConcurrentEntity)
                .ToList();
            foreach (var entry in modifiedEntities)
            {
                if (entry.Entity is not IConcurrentEntity)
                    continue;

                var token = entry.Property(nameof(IConcurrentEntity.ConcurrencyToken));
                var dbToken = ToLong(token.OriginalValue);
                var newToken = ToLong(token.CurrentValue);

                //eğer zaten dbdeki daha büyükse geçmiş olsun reload..
                if (dbToken > newToken)
                {
                    throw _concurrencyExceptionFactory();
                }

                //dbdeki token null ise 1 ile başlatmalıyız
                if (newToken == null
                    || newToken.Value == default)
                {
                    token.CurrentValue = defaultValue;
                }
                else
                {
                    token.CurrentValue = newToken + 1;
                }
            }

            return new ValueTask<InterceptionResult<int>>(result);
        }

        private long? ToLong(object value)
        {
            if (value == null)
                return null;
            try
            {
                return Convert.ToInt64(value);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, e.Message);
                return null;
            }
        }
    }
}