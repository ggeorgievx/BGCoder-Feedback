using System;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;

namespace BGCoderFeedback {
    public static class SlackClient {
        private static readonly Uri _uri = new Uri(@"https://hooks.slack.com/services/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");
        private static readonly Encoding _encoding = new UTF8Encoding();

        private static string slackUsername = "BGCoder-Feedback";
        private static string bgCoderSection = @"BGCoder/Contests/Telerik-Academy-Alpha/Exception-Handling";
        private static string welcomingMessage = $"I will be reporting any changes to your scores in {bgCoderSection}. 0,33% of the time when your score changes you will get a free b33r (#motivation). Good luck, ninjas!";
        private static string slackChannel = "#spam";

        public static void PostMessage(string text, string username = null, string channel = null) {
            Payload payload = new Payload() {
                Channel = channel,
                Username = username,
                Text = text
            };
            PostMessage(payload);
        }

        public static void PostMessage(Payload payload) {
            try {
                string payloadJson = JsonConvert.SerializeObject(payload);
                using (WebClient client = new WebClient()) {
                    NameValueCollection data = new NameValueCollection {
                        ["payload"] = payloadJson
                    };
                    var response = client.UploadValues(_uri, "POST", data);
                    string responseText = _encoding.GetString(response);
                }
            } catch (WebException ex) {
                Console.WriteLine("Bad WebClient");
                throw ex;
            }
        }

        public static void PostWelcomingMessage() {
            PostMessage(username: slackUsername,
                               text: welcomingMessage,
                               channel: slackChannel);
        }

        public static void PostScoreChangeMessage(string messageToDisplay) {
            int randomNumber = new Random().Next(0, 301);
            string toBeAppended = "";
            if (randomNumber >= 0 && randomNumber < 100) {
                toBeAppended = " Congrats!";
            }
            if (randomNumber >= 100 && randomNumber < 200) {
                toBeAppended = " Well done!";
            }
            if (randomNumber >= 200 && randomNumber < 300) {
                toBeAppended = " Bravo!";
            }
            if (randomNumber == 300) {
                toBeAppended = " WOAH! Not only are you smart, but also lucky! Contact Viktor for a free b33r.";
            }
            PostMessage(username: slackUsername,
                               text: messageToDisplay + toBeAppended,
                               channel: slackChannel);
        }
    }

    public class Payload {
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
