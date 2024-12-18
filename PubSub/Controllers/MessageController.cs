using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace PubSub.Controllers
{
    public class MessageController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        public MessageController(IConnectionMultiplexer redis)
        {
            _redis = redis;
            
        }
        private IEnumerable<string> GetMessagesForChannel(string channelName)
        {
            var messages = new List<string>();

            var subscriber = _redis.GetSubscriber(channelName);
            if (subscriber != null)
            {

                subscriber.Subscribe(channelName, (channel, message) =>
            {
                messages.Add(message.ToString());
            });
            }
            //var db = _redis.GetDatabase();
            //return db.ListRange($"channel:{channelName}:messages").Select(m => m.ToString());


            //foreach (var message in messages)
            //{
            //    messages.Add(message.ToString());
            //}

            return messages;
        }
        private void AddMessageToChannel(string channelName, string message)
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.Publish(channelName, message);
        }
        [HttpGet]
        public IActionResult Index()
        {
            var db = _redis.GetDatabase();
            var channels = db.SetMembers("channels").ToStringArray();
            return View(channels);
        }
        [HttpGet]
        public IActionResult ChannelMessages(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                TempData["Error"] = "Channel name cannot be empty.";
                return RedirectToAction("Index");
            }

            var messages = GetMessagesForChannel(channelName);

            TempData["SelectedChannel"] = channelName;
            ViewData["Messages"] = messages;

            return View(messages);
        }
        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            string channelName = TempData["SelectedChannel"].ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction("ChannelMessages", new { channelName });
            }

            //AddMessageToChannel(channelName, message);
            var publisher = _redis.GetSubscriber();
            publisher.Publish(channelName,message);


            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("ChannelMessages", new { channelName });
        }


        [HttpPost]
        public  IActionResult CreateChannel(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                TempData["Error"] = "Channel name cannot be empty.";
                return RedirectToAction("Index");
            }
            var db = _redis.GetDatabase();
            if (!db.SetContains("channels", channelName))
            {
                db.SetAdd("channels", channelName);
            }


            TempData["Success"] = "Channel created successfully and subscribed!";
            return RedirectToAction("Index");
        }
    }
}
