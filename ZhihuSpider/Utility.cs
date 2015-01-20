using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace ZhihuSpider
{
    public class Utility
    {
        
        public static HtmlNodeCollection FindNodesByClass(HtmlAgilityPack.HtmlDocument doc, string classToFind)
        {
            HtmlNodeCollection allElementsWithClass;
            allElementsWithClass = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", classToFind));
            return allElementsWithClass;
        }
        public static HtmlNodeCollection FindNodesByClass(HtmlNode node, string classToFind)
        {
            //If you use //, it searches from the document begin.
            //Use .// to search all from the current node
            return node.SelectNodes(string.Format(".//*[contains(@class,'{0}')]", classToFind));
        }

        public static HtmlNode FindNodeByClass(HtmlNode node, string classToFind)
        {
            //If you use //, it searches from the document begin.
            //Use .// to search all from the current node
            var nodes = FindNodesByClass(node, classToFind);
            if (nodes!=null && nodes.Count()>0)
            {
                return nodes.First();
            }
            return null;
            
        }

        public static DateTime GetTime(HtmlNode node)
        {
            var spanNode = GetSpanInNode(node, "question-item-title");
            double timestamp = double.Parse(spanNode.Attributes["data-timestamp"].Value);

            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(timestamp / 1000d)).ToLocalTime();
        }

        
        public static decimal GetAnswerCount(HtmlNode node)
        {
            var answerLink = GetLinkInNode(node, "question-item-meta");
            if (answerLink != null)
            {
                var answerCount = answerLink.InnerText.Replace(" 个回答", "");
                decimal d = 0;
                if (decimal.TryParse(answerCount, out d))
                {
                    return d;
                }
            }
            return 0;
        }

        public static string GetTitle(HtmlNode node)
        {
            var questionNode = GetLinkInNode(node, "question-item-title");
            return questionNode == null ? "" : questionNode.InnerText;
        }

        public static decimal GetId(HtmlNode node)
        {
            var questionNode = GetLinkInNode(node, "question-item-title");
            var linkUrl = questionNode.Attributes["href"].Value;
            return decimal.Parse(linkUrl.Replace("/question/", ""));
        }

        public static string GetTopTitle(HtmlNode node)
        {
            var questionNode = FindNodeByClass(node, "question_link");
            return questionNode == null ? "" : questionNode.InnerText;
        }
        public static decimal GetTopId(HtmlNode node)
        {
            var questionNode = FindNodeByClass(node, "question_link");
            var linkUrl = questionNode.Attributes["href"].Value;
            return decimal.Parse(linkUrl.Replace("/question/", ""));
        }

        public static string GetTopic(HtmlNode node)
        {

            HtmlNode linkNode = GetLinkInNode(node, "subtopic");

            if (linkNode != null)
            {
                return linkNode.InnerText;
            }
            else
            {
                return string.Empty;
            }
        }

        public static HtmlNode GetLinkInNode(HtmlNode node, string className, int index = 0)
        {
            string tagName = "a";
            HtmlNode linkNode = GetNodeInNode(node, className, index, tagName);
            return linkNode;
        }
        public static HtmlNode GetSpanInNode(HtmlNode node, string className, int index = 0)
        {
            string tagName = "span";
            HtmlNode spanNode = GetNodeInNode(node, className, index, tagName);
            return spanNode;
        }


        public static HtmlNode GetLink(HtmlNode node, string className)
        {
            HtmlNode linkNode = null;
            HtmlNodeCollection allElementsWithClass = FindNodesByClass(node, className);
            if (allElementsWithClass != null && allElementsWithClass.Count > 0)
            {
                linkNode = allElementsWithClass.First();
            }
            return linkNode;
        }

        public static HtmlNode GetNode(HtmlNode node, string className)
        {
            HtmlNode linkNode = null;
            HtmlNodeCollection allElementsWithClass = FindNodesByClass(node, className);
            if (allElementsWithClass != null && allElementsWithClass.Count > 0)
            {
               linkNode = allElementsWithClass.First();
            }
            return linkNode;
        }

        public static HtmlNode GetNodeInNode(HtmlNode node, string className, int index, string tagName)
        {
            HtmlNode linkNode = null;
            HtmlNodeCollection allElementsWithClass = FindNodesByClass(node, className);
            if (allElementsWithClass != null && allElementsWithClass.Count > 0)
            {
                var alllinks = allElementsWithClass[0].Descendants(tagName);
                if (index == 0)
                {
                    linkNode = alllinks.First();
                }
                else if (index == -1)
                {
                    linkNode = alllinks.Last();
                }
                else
                {
                    linkNode = alllinks.ToList()[index];
                }

            }
            return linkNode;
        }

        internal static decimal GetAnswerId(HtmlNode node)
        {
            var answerNode = FindNodeByClass(node, "entry-body");
            var answerId = answerNode.Attributes["data-atoken"].Value;
            return decimal.Parse(answerId);
        }

        internal static decimal GetVoteCount(HtmlNode node)
        {
            // data-votecount
            var voteNumberNode = FindNodeByClass(node, "zm-item-vote");
            var voteNumber = voteNumberNode.FirstChild.Attributes["data-votecount"].Value; //voteNumberNode.InnerText;
            //var replaceK = replaceK(voteNumber);
            return decimal.Parse(voteNumber);
        }


        internal static decimal GetCollectCount(HtmlNode node)
        {
            var sideNodes = FindNodesByClass(node, "zm-side-section-inner");

           

            if (sideNodes!=null)
            {
                var index = 3;
                for (int i = 3; i < sideNodes.Count; i++)
                {
                    if (sideNodes[i].InnerText.Contains("被收藏"))
                    {
                        index = i;
                        break;
                    }
                }

                if (index<sideNodes.Count)
                {
                    var collectNode = sideNodes[index];
                    var innerNode = collectNode.Descendants("h3").First();
                    if (innerNode.Descendants("a").Count() > 0)
                    {
                        var innerLink = innerNode.Descendants("a").First();
                        var collectCount = innerLink.InnerText;
                        return decimal.Parse(collectCount);
                    }
                }
            }
            return 0;
        }

        internal static string GetAnswerAuthor(HtmlNode node)
        {
            var authorNode = FindNodeByClass(node, "zm-item-answer-author-wrap");
            var nodes = authorNode.Descendants("a");
            if (nodes.Count() > 0)
	        {
                return nodes.First().InnerText;
	        }
            else
            {
                return authorNode.InnerText.Trim();
            }
            //return "";
        }

        internal static string GetAnswerAuthorId(HtmlNode node)
        {
            var authorNode = FindNodeByClass(node, "zm-item-answer-author-wrap");
            var nodes = authorNode.Descendants("a");
            if (nodes.Count() > 0)
            {
                var nameNode = nodes.First();
                var idString = nameNode.Attributes["data-tip"].Value;
                var arr = idString.Split(new string[]{"$"}, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Count() > 0)
                {
                    return arr[arr.Count() - 1];
                }
                else
                { 
                    return ""; 
                }
                   
            }
            else
            {
                return "";
            }
        }

        internal static string GetAnswerAuthor2(HtmlNode node)
        {
            var authorNode = FindNodeByClass(node, "zm-item-answer-author-wrap");
            var nodes = authorNode.Descendants("a");
            if (nodes.Count() > 1)
	        {
                return nodes.ElementAt(1).InnerText;
	        }
            else
            {
                return "";// authorNode.InnerText.Trim();
            }
            //return "";
        }
        

        internal static DateTime? GetAnswerTime(HtmlNode node)
        {
            //answer-date-link-wrap
            var dateNode = FindNodeByClass(node, "answer-date-link");            

            DateTime dt =DateTime.Today;
            
            if (dateNode!=null)
            {
                var dateStr = dateNode.InnerText.Trim(); 
                var forParse = GetSection(dateStr, 1);
                if (forParse != null)
                {
                    if (! DateTime.TryParse(forParse, out dt))
                    {
                        dt = DateTime.Today; 
                    }
                }
            }            

            return dt;
        }

        private static string GetSection(string dateStr, int index)
        {
            string forParse = null;
            var arr = dateStr.Split(' ');
            if (arr.Length > 1)
            {
                forParse = arr[index];
            }
            return forParse;
        }

        internal static decimal GetCommentCount(HtmlNode node)
        {
            var commentNodes = FindNodesByClass(node, "toggle-comment");
            if (commentNodes!=null && commentNodes.Count > 1)
            {
                var commentNode = commentNodes[1];
                var forParse = GetSection(commentNode.InnerText.Trim(), 0);
                if (forParse!=null)
                {
                    return decimal.Parse(forParse);
                }
            }
            return 0;
        }

    }
}
