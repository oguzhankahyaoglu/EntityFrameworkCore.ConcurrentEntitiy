namespace EntityFrameworkCore.ConcurrentEntitiy
{
    /// <summary>
    /// </summary>
    public interface IConcurrentEntity
    {
        /// <summary>
        /// Interceptor tarafından yönetilecektir
        /// </summary>
        long? ConcurrencyToken { get; set; }
    }
}