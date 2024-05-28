using FastBIRe.Cdc.Events;

namespace FastBIRe.CdcSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new MySqlTester().Start();
        }

        public static void Vars_EventRaised(object? sender, CdcEventArgs e)
        {
            Console.WriteLine(e.Checkpoint);
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
                            Console.WriteLine($"UpdateData:{(item.BeforeRow == null ? null : string.Join(",", item.BeforeRow))} -> {string.Join(",", item.AfterRow)}");
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