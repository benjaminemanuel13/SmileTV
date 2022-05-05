using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmileTV.UIModels;
using System.Net.Mime;
using System.IO;
using Shared.Models;

namespace SmileTV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalkthroughApiController : ControllerBase
    {
        //private readonly IWebHostEnvironment _webHostEnvironment;
        private string webroot = "D:\\Projects\\TheWorks\\SmileTV\\SmileTV\\wwwroot";

        //public WalkthroughApiController(IWebHostEnvironment webHostEnvironment)
        //{
        //    _webHostEnvironment = webHostEnvironment;,
        //    webroot = _webHostEnvironment.WebRootPath
        //}

        [HttpGet("GetMenu")]
        public IEnumerable<NavButton> GetMenu(string menuName = "")
        {
            var menus = GetMenus().Where(x => x.name == menuName).ToList();

            if (menus.Count() > 0)
            {
                return menus.First().items;
            }

            return new List<NavButton>();
        }

        [HttpPost("SetButtonName")]
        public bool SetButtonName(string menuName, int buttonPos, string newCaption)
        {
            var menus = GetMenus();

            var menu = menus.Where(x => x.name.Equals(menuName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var item = menu.items[buttonPos];
            item.caption = newCaption;

            string json = JsonConvert.SerializeObject(menus);

            //FileStream stream = new FileStream(_webHostEnvironment.WebRootPath + "\\Walkthrough\\menu.json", FileMode.Create);
            FileStream stream = new FileStream(webroot + "\\Walkthrough\\menu.json", FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);

            writer.Write(json);

            writer.Close();
            stream.Close();

            return false;
        }

        private List<Menu> GetMenus()
        {
            FileStream stream = new FileStream(webroot + "\\Walkthrough\\menu.json", FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            string json = reader.ReadToEnd();

            reader.Close();
            stream.Close();

            var menus = JsonConvert.DeserializeObject<List<Menu>>(json);
            return menus;
        }

        [HttpGet("/ws/clientcrap")]
        public async Task<bool> Client()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var socketFinishedTcs = new TaskCompletionSource<object>();

                var server = Control.Servers["Server1"];
                Client client = new Client(socketFinishedTcs, webSocket);

                server.Clients.Add(client);

                await socketFinishedTcs.Task;

                Control.Servers["Server1"].Clients.Remove(client);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            return true;
        }

        [HttpGet("GetVideo")]
        public FileResult GetVideo(string videoName = "Welcome.mp4")
        {
            try
            {
                FileStream stream = new FileStream(webroot + "\\Walkthrough\\" + videoName, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);

                byte[] bytes = reader.ReadBytes((int)stream.Length);

                reader.Close();
                stream.Close();

                return File(bytes, "video/mp4");
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("StartKinesis")]
        public bool StartKinesis()
        {
            return true;
        }
    }
}
