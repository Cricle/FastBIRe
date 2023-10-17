using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Mssql;
using Microsoft.Data.SqlClient;
using MongoDB.Bson;
using MongoDB.Driver;
using rsa;

namespace FastBIRe.CdcSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mongo = new MongoClient("mongodb://localhost:27017/admin");
            var db = mongo.GetDatabase("test");
            //var command = new BsonDocument { { "replSetGetStatus", 1 } };
            //var result = mongo.GetDatabase("admin").RunCommand<BsonDocument>(command);
            var a = db.Watch(new ChangeStreamOptions
            {
                  FullDocument=  ChangeStreamFullDocumentOption.UpdateLookup
            });
            while (await a.MoveNextAsync())
            {
                if (a.Current.Any())
                {

                }
            }
            Console.ReadLine();
        }
        private static void Dep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            Console.WriteLine($"{e.Info},{e.Type},{e.Source}");
        }

        public static void Vars_EventRaised(object? sender, CdcEventArgs e)
        {
            switch (e)
            {
                case TableMapEventArgs tme:
                    {
                        Console.WriteLine($"TableMap:{tme.TableInfo?.DatabaseName}.{tme.TableInfo?.TableName} -> {tme.TableInfo?.Id}");
                    }
                    break;
                case InsertEventArgs iea:
                    {
                        foreach (var item in iea.Rows)
                        {
                            Console.WriteLine($"InsertData:{string.Join(",", item)}");
                        }
                    }
                    break;
                case UpdateEventArgs uea:
                    {
                        foreach (var item in uea.Rows)
                        {
                            Console.WriteLine($"UpdateData:{(item.BeforeRow==null?null:string.Join(",", item.BeforeRow))} -> {string.Join(",", item.AfterRow)}");
                        }
                    }
                    break;
                case DeleteEventArgs dea:
                    {
                        foreach (var item in dea.Rows)
                        {
                            Console.WriteLine($"DeleteData:{string.Join(",", item)}");
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}