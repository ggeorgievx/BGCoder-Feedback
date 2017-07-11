using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;

namespace BGCoderFeedback {
    class BGCoderFeedback {
        static List<string> users = new List<string>();
        static List<int> scores = new List<int>();
        static List<string> newUsers = new List<string>();
        static List<int> newScores = new List<int>();

        static string bgCoderUrl = @"http://bgcoder.com/Contests/Practice/Results/Simple/321";
        static string cookieValue = @"";
        static int exercisesCount = 2;

        public static void Main() {
            SlackClient.PostWelcomingMessage();
            int requestsCount = 0;
            while (requestsCount < 1000) {
                try {
                    StreamReader objReader = HTTPWebRequest();
                    ParseHTML(objReader);
                    PostAnyScoreChanges();
                    ResetLists();
                    Console.WriteLine(new string('*', 20));
                    Console.WriteLine("RequestsCount: " + requestsCount);
                    requestsCount++;
                    Thread.Sleep(5000);
                } catch (WebException ex) {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    Console.WriteLine(response == null ? HttpStatusCode.InternalServerError : response.StatusCode);
                }
            }
        }

        static StreamReader HTTPWebRequest() {
            try {
                Cookie cookie = new Cookie(".AspNet.ApplicationCookie", cookieValue);
                Uri uri = new Uri(bgCoderUrl);
                HttpWebRequest request = WebRequest.Create(bgCoderUrl) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(uri, cookie);
                Stream objStream = request.GetResponse().GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                return objReader;
            } catch (WebException ex) {
                Console.WriteLine("Bad HTTPWebRequest");
                throw ex;
            }
        }

        static void ParseHTML(StreamReader objReader) {
            string currentLine = "";
            int currentLineNumber = 0;
            int lineContainingScore = -1;
            while (currentLine != null) {
                currentLine = objReader.ReadLine();
                if (currentLine != null && currentLine.Contains(@"/Users/Profile?")) {
                    lineContainingScore = currentLineNumber + exercisesCount + 2;
                    int indexOfEquals = currentLine.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
                    int indexOfSecondEquals = currentLine.IndexOf("=", indexOfEquals + 1, StringComparison.InvariantCultureIgnoreCase);
                    int indexOfQuote = currentLine.IndexOf("\"", indexOfSecondEquals + 1, StringComparison.InvariantCultureIgnoreCase);
                    string currentUser = currentLine.Substring(indexOfSecondEquals + 1, indexOfQuote - indexOfSecondEquals - 1);
                    if (currentUser == " ") {
                        continue;
                    }
                    if (!users.Contains(currentUser)) {
                        users.Add(currentUser);
                    }
                    newUsers.Add(currentUser);
                }
                if (currentLineNumber == lineContainingScore) {
                    int indexOfTag = currentLine.IndexOf("<td>", StringComparison.InvariantCultureIgnoreCase);
                    int indexOfForwardSlash = currentLine.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                    int currentScore = int.Parse(currentLine.Substring(indexOfTag + "<td>".Length, indexOfForwardSlash - indexOfTag - "<td>".Length - 1));
                    if (scores.Count < users.Count) {
                        scores.Add(currentScore);
                    }
                    newScores.Add(currentScore);
                }
                currentLineNumber++;
            }
        }

        static void PostAnyScoreChanges() {
            for (int i = 0; i < users.Count; i++) {
                int newIndex = newUsers.IndexOf(users[i]);
                if (scores[i] != newScores[newIndex]) {
                    int scoreChange = newScores[newIndex] - scores[i];
                    string textToDisplay = String.Format("User {0} had {1} points. He gained {2} points and now has {3}.", users[i], scores[i], scoreChange, newScores[newIndex]);
                    SlackClient.PostScoreChangeMessage(textToDisplay);
                }
            }
        }

        static void ResetLists() {
            users = newUsers;
            scores = newScores;
            newUsers = new List<string>();
            newScores = new List<int>();
        }
    }

    public class SlackClient {
        readonly Uri _uri;
        readonly Encoding _encoding = new UTF8Encoding();

        static string slackUrlWithAccessToken = @"https://hooks.slack.com/services/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";

        static readonly SlackClient client = new SlackClient(slackUrlWithAccessToken);

        static string slackUsername = "BGCoder-Feedback";
        static string bgCoderSection = @"BGCoder/Contests/Telerik-Academy-Alpha/Exception-Handling";
        static string welcomingMessage = String.Format("I will be reporting any changes to your scores in {0}. 0,33% of the time when your score changes you will get a free b33r (#motivation). Good luck, ninjas!", bgCoderSection);
        static string slackChannel = "#spam";

        public SlackClient(string urlWithAccessToken) {
            _uri = new Uri(urlWithAccessToken);
        }

        public void PostMessage(string text, string username = null, string channel = null) {
            Payload payload = new Payload() {
                Channel = channel,
                Username = username,
                Text = text
            };
            PostMessage(payload);
        }

        public void PostMessage(Payload payload) {
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
            client.PostMessage(username: slackUsername,
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
            client.PostMessage(username: slackUsername,
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
