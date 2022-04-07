using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ConcurrentEntitiy
{
    public class ConcurrentEntityException : DbUpdateConcurrencyException
    {
        public ConcurrentEntityException() :
            base("Başka bir kullanıcı verileri değiştirmiştir, lütfen ekranınızı yenileyiniz.")
        {
        }
    }
}