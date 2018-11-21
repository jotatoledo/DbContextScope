namespace EntityFrameworkCore.DbContextScope
{
    internal interface IEntityRefresh
    {
        void Refresh<TEntity>(TEntity toRefresh);
    }
}
