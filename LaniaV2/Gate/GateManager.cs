using Discord.WebSocket;
using Google.Cloud.Vision.V1;
using LaniaV2.Core;
using System.Linq;
using System.Net;

namespace LaniaV2.Gate
{
    public class GateManager
    {
        public GateManager()
        {
            imageClient = ImageAnnotatorClient.Create();
        }

        public void SendToRandomGates(Guild guild, SocketUserMessage msg)
        {
            string url = GetImageUrl(msg);
            if (!IsUrlImage(url)) // URL is not an image
                return;
        }

        private string GetImageUrl(SocketUserMessage msg)
        {
            if (msg.Attachments.Count > 0)
                return msg.Attachments.ToArray()[0].Url;
            return msg.Content;
        }

        private bool IsUrlImage(string url)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower().StartsWith("image/");
            }
        }

        private ImageAnnotatorClient imageClient;
    }
}
