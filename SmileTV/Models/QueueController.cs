using EasyNetQ;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Shared.Models;
using SmileTV.Hubs;
using SmileTV.UIModels;
using System.Diagnostics;

namespace SmileTV.Models
{
    public class QueueController
    {
        public static IHubContext<QueueHub> Hub { get; set; }
        private readonly IWebHostEnvironment _env;
        private readonly Random rnd = null;
        
        private object obj = new object();

        static public TaskCompletionSource<object> task { get; } = new TaskCompletionSource<object>();

        public QueueController(IWebHostEnvironment env, IHubContext<QueueHub> queue)
        {
            _env = env;
            rnd = new Random();
            Hub = queue;
        }

        public async void StartUp()
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                //, x => x.WithTopic("my.topic")
                await bus.PubSub.SubscribeAsync<ButtonUpdate>("buttonUpdate", HandleButtonUpdate);

                bus.Rpc.Respond<RandomRequest, RandomResponse>(Responder);
                await bus.Rpc.RespondAsync<RandomRequest, RandomResponse>(Responder);

                bus.SendReceive.Receive<RecieveOne>("recieve-one", Recieve);
                bus.SendReceive.Receive("recieve-one", x => x.Add<RecieveOne>(Recieve).Add<RecieveTwo>(RecieveTwo));

                await bus.PubSub.SubscribeAsync<TopicOne>("topics", TopicOneRecieve, x => x.WithTopic("topic.topic-one"));
                await bus.PubSub.SubscribeAsync<TopicOne>("topics", TopicTwoRecieve, x => x.WithTopic("topic.topic-two"));
                await bus.PubSub.SubscribeAsync<TopicOne>("topics", TopicAllRecieve, x => x.WithTopic("topic.*"));

                KinesisController.Instance.MessageRecieved += Instance_MessageRecieved;

                await task.Task;
            }
        }

        private void Instance_MessageRecieved(object? sender, EventArguments.KinesisDataRecievedEventArgs e)
        {
            //Would also persist message or do whatever with it, these messages will be lost
            //if noone is looking at the Walkthrough page.

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage(e.Message));
        }

        void TopicOneRecieve(TopicOne topic)
        {
            Debug.WriteLine("Topic One: " + topic.Name);

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Topic 1: " + topic.Name));

            CountController.Counters["topics_total_calls"].Inc();
        }

        void TopicTwoRecieve(TopicOne topic)
        {
            Debug.WriteLine("Topic Two: " + topic.Name);

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Topic 2: " + topic.Name));

            CountController.Counters["topics_total_calls"].Inc();
        }

        void TopicAllRecieve(TopicOne topic)
        {
            //Should get both
            Debug.WriteLine("Topic All: " + topic.Name);

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Topic All: " + topic.Name));

            CountController.Counters["topics_total_calls"].Inc();
        }

        private void Recieve(RecieveOne rec)
        {
            try
            {
                Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Send/Recieve: " + rec.Name));
                CountController.Counters["recieve_total_calls"].Inc();
            }
            catch
            { 
            }
        }

        private void RecieveTwo(RecieveTwo rec)
        {
        }

        private RandomResponse Responder(RandomRequest req)
        {
            var res = new RandomResponse() {
                Number = rnd.Next(req.Min, req.Max)
            };

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Random: " + res.Number));

            CountController.Counters["random_total_calls"].Inc();

            return res;
        }

        private void HandleButtonUpdate(ButtonUpdate update)
        {
            string menuName = update.menuName;
            int buttonPos = update.pos;
            string newCaption = update.newCaption;

            var menus = GetMenus();

            var menu = menus.Where(x => x.name.Equals(menuName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var item = menu.items[buttonPos];

            string oldCaption = item.caption;

            item.caption = newCaption;

            string json = JsonConvert.SerializeObject(menus);

            FileStream stream = new FileStream(_env.WebRootPath + "\\Walkthrough\\menu.json", FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);

            writer.Write(json);

            writer.Close();
            stream.Close();

            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("Button Update: " + oldCaption + " > " + newCaption));
            Control.Servers["Server1"].Clients.ForEach(x => x.SendMessage("[UPDATE]" + update.menuName + "," + update.pos + "," + update.newCaption));

            CountController.Counters["button_total_calls"].Inc();
        }

        private List<Menu> GetMenus()
        {
            FileStream stream = new FileStream(_env.WebRootPath + "\\Walkthrough\\menu.json", FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            string json = reader.ReadToEnd();

            reader.Close();
            stream.Close();

            var menus = JsonConvert.DeserializeObject<List<Menu>>(json);
            return menus;
        }
    }
}
