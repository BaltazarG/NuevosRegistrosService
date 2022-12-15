using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NuevosRegistrosService
{
    public class AzServiceBus : IAzServiceBus
    {
        private readonly ServiceBusClient _serviceBusClient;
        public AzServiceBus(ServiceBusClient service)
        {
            _serviceBusClient = service;
        }

        public async Task SendMessageAsync(CarModel modelMessage)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender("myqueue");

            var body = System.Text.Json.JsonSerializer.Serialize(modelMessage);

            var sbMessage = new ServiceBusMessage(body);

            await sender.SendMessageAsync(sbMessage);
        }

        public async Task GetNewData(CancellationToken stoppingToken)
        {
            string CONNSTR = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

            SqlConnection conn = new SqlConnection(CONNSTR);
            conn.Open();
            SqlCommand command = new SqlCommand("Select Id, Name, Color, CarId, IsNew FROM [NewCarsDone] WHERE IsNew = 1", conn);
            // int result = command.ExecuteNonQuery();
            var cars = new List<CarModelWithId>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var car = new CarModel()
                    {
                        Color = reader["Color"]?.ToString(),
                        Name = reader["Name"]?.ToString()
                    };

                    await SendMessageAsync(car);

                    var carWithId = new CarModelWithId()
                    {
                        Color = reader["Color"]?.ToString(),
                        Name = reader["Name"]?.ToString(),
                        Id = Int32.Parse(reader["Id"]?.ToString())
                    };
                    cars.Add(carWithId);
                    //Console.WriteLine(String.Format("{0}", reader["Id"]));
                }
            }
            using (SqlCommand command2 = conn.CreateCommand())
            {
                foreach (var item in cars)
                {
                    command2.CommandText = $"UPDATE NewCarsDone SET IsNew = 0 WHERE Id = {item.Id}";
                    command2.ExecuteNonQuery();
                }
            }
            conn.Close();
        }
    }
}
