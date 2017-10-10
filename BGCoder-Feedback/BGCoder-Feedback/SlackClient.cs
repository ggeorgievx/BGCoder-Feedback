using System;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace BGCoderFeedback
{
    public static class SlackClient
    {
        private static readonly Uri _uri = new Uri(@"https://hooks.slack.com/services/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");

        private const string SlackUsername = "BGCoder-Feedback";
        private const string BgCoderSection = @"BGCoder/Contests/Telerik-Academy-Alpha/Exception-Handling";
        private static readonly string welcomingMessage = $"I will be reporting any changes to your scores in {BgCoderSection}. 0,33% of the time when your score changes you will get a free b33r (#motivation). Good luck, ninjas!";
        private const string SlackChannel = "#spam";

        private const string WebExceptionString = "Bad WebClient";

        private const string Greet1 = " Congrats!";
        private const string Greet2 = " Well done!";
        private const string Greet3 = " Bravo!";
        private const string Greet4 = " WOAH! Not only are you smart, but also lucky! Contact Viktor for a free b33r.";

        public static void PostMessage(string text, string username = null, string channel = null)
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };
            PostMessage(payload);
        }

        public static void PostMessage(Payload payload)
        {
            try
            {
                string payloadJson = JsonConvert.SerializeObject(payload);
                using (WebClient client = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection
                    {
                        ["payload"] = payloadJson
                    };
                    var response = client.UploadValues(_uri, "POST", data);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(WebExceptionString);
                throw ex;
            }
        }

        public static void PostWelcomingMessage()
        {
            PostMessage(username: SlackUsername,
                               text: welcomingMessage,
                               channel: SlackChannel);
        }

        public static void PostScoreChangeMessage(string messageToDisplay)
        {
            int randomNumber = new Random().Next(0, 301);
            string toBeAppended = "";
            if (randomNumber >= 0 && randomNumber < 100)
            {
                toBeAppended = Greet1;
            }
            if (randomNumber >= 100 && randomNumber < 200)
            {
                toBeAppended = Greet2;
            }
            if (randomNumber >= 200 && randomNumber < 300)
            {
                toBeAppended = Greet3;
            }
            if (randomNumber == 300)
            {
                toBeAppended = Greet4;
            }
            PostMessage(username: SlackUsername,
                               text: messageToDisplay + toBeAppended,
                               channel: SlackChannel);
        }
    }
}