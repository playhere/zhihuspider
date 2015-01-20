using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace ZhihuSpider
{
    public partial class Form2 : Form
    {

        Model1Container db = new Model1Container();
        HtmlWeb htmlWeb = new HtmlWeb();
        string latestUrl = string.Empty;
        bool ready = false;
        bool readyDetail = false;

        public Form2()
        {
            InitializeComponent();
        }

        // http://www.zhihu.com/topic/19776749/top-answers 根话题精华
        // 答案格式 data-atoken="16245159" 答案编号
        // 问题格式 <a class="question_link" target="_blank" href="/question/19568396">大家觉得自己牛逼在哪？</a>

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var oldIds = from t in db.QuestionSet select t.Id;
            var oldAnswerIds = from t in db.AnswerSet select t.Id;

            List<decimal> newIds = new List<decimal>();
            List<decimal> newAnswerIds = new List<decimal>();
            //WebBrowser browser = new WebBrowser();
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
            browserDetail.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browserDetail_DocumentCompleted);

            var pageCount = (int)numericUpDown1.Value;
            
            for (int i = pageCount; i > 0; i--)
            {
                var url = string.Format("http://www.zhihu.com/topic/19776749/top-answers?page={0}", i);
                
                //NetworkCredential c = new NetworkCredential("playhere@126.com","password");
                //HtmlAgilityPack.HtmlDocument doc =  htmlWeb.Load(url,"get",null,c);

                browser.Navigate(url);
                while (!ready)
                {
                    Application.DoEvents();
                }
                //System.Threading.Thread.Sleep(1000);
                ready = false;
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(browser.Document.Body.OuterHtml);

                var nodes = Utility.FindNodesByClass(doc, "feed-main");
                foreach (var node in nodes)
                {
                    //先找问题
                    Question q = new Question();
                    q.Id = Utility.GetTopId(node);
                    //q.AnswerCount = GetAnswerCount(node); 拿不到，html里面没有
                    //q.CreateTime = Utility.GetTopTime(node);
                    //q.FollowerCount = GetFollower(node);
                    q.Title = Utility.GetTopTitle(node);
                    q.Topic = Utility.GetTopic(node);
                    //q.ViewCount = GetViewCount(node);

                    bool contain = newIds.Contains(q.Id) || oldIds.Contains(q.Id);

                    if (!contain)
                    {
                        db.QuestionSet.AddObject(q);
                        newIds.Add(q.Id);

                        // Application.DoEvents();
                    }
                    Application.DoEvents();
                    txtTitle.Text = string.Format("P:{0},T:{1}", i, q.Title);

                    //再找答案
                    var answerId = Utility.GetAnswerId(node);
                    Answer a;
                    bool containAnswer = newAnswerIds.Contains(answerId) || oldAnswerIds.Contains(answerId);
                    if (!containAnswer)
                    {
                        a = new Answer();
                        a.Id = answerId;
                        db.AnswerSet.AddObject(a);
                        newIds.Add(a.Id);
                        //Application.DoEvents();
                    }
                    else
                    {
                        a = db.AnswerSet.First(t => t.Id == answerId);
                    }

                    a.Voteup = Utility.GetVoteCount(node);
                    a.Author = Utility.GetAnswerAuthor(node);
                    a.AuthorId = Utility.GetAnswerAuthorId(node);
                   
                    var answerUrl = string.Format("http://www.zhihu.com/question/{0}/answer/{1}", q.Id, a.Id);

                    
                    //var answerDoc = htmlWeb.Load(answerUrl);
                    browserDetail.Navigate(answerUrl);
                    while (!readyDetail)
                    {
                        Application.DoEvents();
                    }
                    readyDetail = false;
                    HtmlAgilityPack.HtmlDocument answerDoc = new HtmlAgilityPack.HtmlDocument();
                    answerDoc.LoadHtml(browserDetail.Document.Body.OuterHtml);

                    var rootNode = answerDoc.DocumentNode;
                    //var author = Utility.GetAnswerAuthor2(rootNode);
                    //if ( string.IsNullOrEmpty(a.Author) && !string.IsNullOrEmpty(author))
                    //{
                    //    a.Author = author;
                    //}
                    a.CollectCount = Utility.GetCollectCount(rootNode);
                    a.CreateTime = Utility.GetAnswerTime(rootNode);
                    a.CommentCount = Utility.GetCommentCount(rootNode);
                    a.QuestionId = q.Id;
                }

                db.SaveChanges();

            }
            MessageBox.Show("Over!");

        }

        void browserDetail_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;
            readyDetail = true;
        }

        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;
            ready = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var report = (from t in db.AnswerSet
                          join s in db.QuestionSet on t.QuestionId equals s.Id
                          select new { t.CollectCount, t.QuestionId, t.Id, s.Title, t.Author }).OrderByDescending(t => t.CollectCount).Take((int)numericUpDown1.Value * 20);
            var sb = new StringBuilder();
            int i = 1;
            foreach (var item in report)
	        {
                //sb.Append(string.Format("1	47437	<a href="http://www.zhihu.com/question/23009666/answer/23368714" class="internal">知乎上关于男士的精彩问答有哪些？</a>	李傲文"));
                sb.Append(string.Format("{0} {1} <a href=\"http://www.zhihu.com/question/{2}/answer/{3}\">{4}</a> @{5}<br>\n",
                    i++,item.CollectCount,item.QuestionId,item.Id,Brief(item.Title),item.Author));
	        }
            txtResult.Text = sb.ToString();

            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "htm";
            if (dlg.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter tw=new StreamWriter(dlg.FileName) )
                {
                    tw.Write(sb.ToString());
                }
            }
            
        }

        private string Brief(string p)
        {
            if (p.Length>35)
            {
                return p.Substring(0, 35)+"...";
            }
            else
            {
                return p;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var report = from t in db.AnswerSet
                         join s in db.QuestionSet on t.QuestionId equals s.Id
                         where t.CollectCount > 1000 && t.Author != "匿名用户"
                         group t.CollectCount by new { t.Author, t.AuthorId } into g
                         select new { Author = g.Key.Author,AuthorId=g.Key.AuthorId, Count = g.Count(),TotalCollect =g.Sum()  };
            report = report.OrderByDescending(t => t.Count).ThenByDescending(t=>t.TotalCollect);
            var sb = new StringBuilder();
            int i = 1;
            foreach (var item in report)
            {
                //<a href="http://www.zhihu.com/people/{3}" >@{2}</a>
                sb.Append(string.Format("{0} {1} <a href='http://www.zhihu.com/people/{3}'>@{2}</a> {4}<br>\n",
                    i++, item.Count, item.Author,item.AuthorId,item.TotalCollect));
            }
            txtResult.Text = sb.ToString();

            var dlg = new SaveFileDialog();
            dlg.DefaultExt = "htm";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter tw = new StreamWriter(dlg.FileName))
                {
                    tw.Write(sb.ToString());
                }
            }

        }
        
    }
}


