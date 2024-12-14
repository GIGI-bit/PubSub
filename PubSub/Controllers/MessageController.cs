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
            var db = _redis.GetDatabase();
            var messages = new List<string>();

            var length = db.ListLength(channelName);
            var range = db.ListRange(channelName, Math.Max(0, length - 10), length - 1);

            foreach (var message in range)
            {
                messages.Add(message.ToString());
            }

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

            ViewData["SelectedChannel"] = channelName;
            ViewData["Messages"] = messages;

            return View(messages);
        }
        [HttpPost]
        public IActionResult SendMessage(string channelName, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction("ChannelMessages", new { channelName });
            }

            AddMessageToChannel(channelName, message);

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("ChannelMessages", new { channelName });
        }


        [HttpPost]
        public IActionResult CreateChannel(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                TempData["Error"] = "Channel name cannot be empty.";
                return RedirectToAction("Index");
            }

            var db = _redis.GetSubscriber();

            db.Subscribe(channelName, (channel, message) =>
            {
                Console.WriteLine($"Message received on channel {channel}: {message}");
            });

            TempData["Success"] = "Channel created successfully and subscribed!";
            return RedirectToAction("Index");
        }
    }
}
