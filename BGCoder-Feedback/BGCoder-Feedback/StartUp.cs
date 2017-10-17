using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace BGCoderFeedback
{
    public static class StartUp
    {
        private static List<string> users = new List<string>();
        private static List<int> scores = new List<int>();
        private static List<string> newUsers = new List<string>();
        private static List<int> newScores = new List<int>();

        private const string BgCoderUrl = @"http://bgcoder.com/Contests/Practice/Results/Simple/321";
        private const string CookieValue = @"";
        private const int ExercisesCount = 2;

        private static readonly string lineBreak = new string('*', 20);
        private const string RequestsCountString = "RequestsCount: ";
        private const string CookieId = ".AspNet.ApplicationCookie";
        private const string WebExceptionString = "Bad HTTPWebRequest";

        public static void Main()
        {
            try
            {
                SlackClient.PostWelcomingMessage();
                int requestsCount = 0;
                while (requestsCount < 1000)
                {
                    StreamReader objReader = HTTPWebRequest();
                    ParseHTML(objReader);
                    PostAnyScoreChanges();
                    ResetLists();
                    Console.WriteLine(lineBreak);
                    Console.WriteLine(RequestsCountString + requestsCount);
                    requestsCount++;
                    Thread.Sleep(5000);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                Console.WriteLine(response == null ? HttpStatusCode.InternalServerError : response.StatusCode);
            }
        }

        public static StreamReader HTTPWebRequest()
        {
            try
            {
                Cookie cookie = new Cookie(CookieId, CookieValue);
                Uri uri = new Uri(BgCoderUrl);
                HttpWebRequest request = WebRequest.Create(BgCoderUrl) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(uri, cookie);
                Stream objStream = request.GetResponse().GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                return objReader;
            }
            catch (WebException ex)
            {
                Console.WriteLine(WebExceptionString);
                throw ex;
            }
        }

        public static void ParseHTML(StreamReader objReader)
        {
            int currentLineNumber = 0;
            int lineContainingScore = -1;
            while (true)
            {
                string currentLine = objReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                if (currentLine != null && currentLine.Contains(@"/Users/Profile?"))
                {
                    lineContainingScore = currentLineNumber + ExercisesCount + 2;
                    int indexOfEquals = currentLine.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
                    int indexOfSecondEquals = currentLine.IndexOf("=", indexOfEquals + 1, StringComparison.InvariantCultureIgnoreCase);
                    int indexOfQuote = currentLine.IndexOf("\"", indexOfSecondEquals + 1, StringComparison.InvariantCultureIgnoreCase);
                    string currentUser = currentLine.Substring(indexOfSecondEquals + 1, indexOfQuote - indexOfSecondEquals - 1);
                    if (!users.Contains(currentUser))
                    {
                        users.Add(currentUser);
                    }
                    newUsers.Add(currentUser);
                }
                if (currentLineNumber == lineContainingScore)
                {
                    int indexOfTag = currentLine.IndexOf("<td>", StringComparison.InvariantCultureIgnoreCase);
                    int indexOfForwardSlash = currentLine.IndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                    int currentScore = int.Parse(currentLine.Substring(indexOfTag + "<td>".Length, indexOfForwardSlash - indexOfTag - "<td>".Length - 1));
                    if (scores.Count < users.Count)
                    {
                        scores.Add(currentScore);
                    }
                    newScores.Add(currentScore);
                }
                currentLineNumber++;
            }
        }

        public static void PostAnyScoreChanges()
        {
            for (int i = 0; i < users.Count; i++)
            {
                int newIndex = newUsers.IndexOf(users[i]);
                if (scores[i] != newScores[newIndex])
                {
                    int scoreChange = newScores[newIndex] - scores[i];
                    string textToDisplay = $"User {users[i]} had {scores[i]} points. He gained {scoreChange} points and now has {newScores[newIndex]}.";
                    SlackClient.PostScoreChangeMessage(textToDisplay);
                }
            }
        }

        public static void ResetLists()
        {
            users = newUsers;
            scores = newScores;
            newUsers = new List<string>();
            newScores = new List<int>();
        }
    }
}
