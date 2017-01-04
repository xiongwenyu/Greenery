using Greenery.Event.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Greenery.Event.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var pub = PublisherFactory.Create();
            pub.Initialization(false);
            Console.WriteLine("输入S订阅，否则为推送！");

            if (Console.ReadLine().ToUpper() == "S")
            {
                pub.Subscribe<ObjectsEvent<TestEvent>>((t) =>
              {
                  Console.WriteLine(t.CreateDt + " " + t.PublisherId + "  " + t.Content.Content);
              }, ObjectModels.FilterMode.StartsWith, "Greenery", "TestEvent");
                Console.WriteLine("订阅完成！");
            }
            else
            {
                int i = 0;
                while (true)
                {
                    i++;
                    pub.Publish(new ObjectModels.EventDescription("Greenery", "TestEvent"), new TestEvent() { Content = "第" + i.ToString() + "条消息！" });
                    if (i > 100000) break;
                    Thread.Sleep(10);
                }
                Console.WriteLine("推送完成！");
            }

            Console.ReadLine();
        }
    }

    public class TestEvent
    {
        public string Content { get; set; }
    }
}
