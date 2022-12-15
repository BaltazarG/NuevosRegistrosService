using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuevosRegistrosService
{
    public interface IAzServiceBus
    {
        public Task GetNewData(CancellationToken stoppingToken);
        public Task SendMessageAsync(CarModel modelMessage);

    }
}
