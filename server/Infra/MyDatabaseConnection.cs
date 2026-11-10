using LinqToDB;
using LinqToDB.Data;

namespace Infra;

public class MyDatabaseConnection(DataOptions<MyDatabaseConnection> options)
    : DataConnection(options.Options)
{
    public ITable<User> Users => this.GetTable<User>();
}