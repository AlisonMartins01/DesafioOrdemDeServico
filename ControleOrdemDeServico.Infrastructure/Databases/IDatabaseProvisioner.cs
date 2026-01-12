using System;
using System.Collections.Generic;
using System.Text;

namespace OsService.Infrastructure.Databases
{
    public interface IDatabaseProvisioner
    {
        Task EnsureCreatedAsync(CancellationToken ct = default);
    }
}
