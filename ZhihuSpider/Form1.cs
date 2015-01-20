using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HAP = HtmlAgilityPack;
using HtmlAgilityPack;

namespace ZhihuSpider
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        HtmlWeb htmlWeb = new HtmlWeb();
        Model1Container db = new Model1Container();

        private void button1_Click(object sender, EventArgs e)
        {
            HtmlAgilityPack.HtmlDocument doc = htmlWeb.Load("http://www.zhihu.com/topic/19776749/questions");

            //get the footer pager,class = zm-invite-pager
            HtmlNode node = doc.DocumentNode;

            HtmlNodeCollection allElementsWithClass = Utility.FindNodesByClass(doc, "zm-invite-pager");
            if (allElementsWithClass.Count>0)
            {
                HtmlNode pagerNode = allElementsWithClass[0];
                var allPages = pagerNode.Descendants("span").ToList();
                var pageCount = allPages[allPages.Count - 2].InnerText;
                txtPageCount.Text = pageCount;
                txtQuestionCount.Text =string.Format("{0}", int.Parse(pageCount) * 20);
            }

            
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            var oldIds = from t in db.QuestionSet select t.Id;
            List<decimal> newIds = new List<decimal>();

            if (txtPageCount.Text.Length>0)
            {
                var pageCount = int.Parse(txtPageCount.Text);
                for (int i = pageCount; i >= 0; i--)
                {
                    var url = string.Format("http://www.zhihu.com/topic/19776749/questions?page={0}",i);
                    HtmlAgilityPack.HtmlDocument doc = htmlWeb.Load(url);

                    var nodes = Utility.FindNodesByClass(doc, "question-item");
                    foreach (var node in nodes)
                    {
                        Question q = new Question();
                        q.Id = Utility.GetId(node);
                        //q.AnswerCount = GetAnswerCount(node); 拿不到，html里面没有
                        q.CreateTime = Utility.GetTime(node);
                        //q.FollowerCount = GetFollower(node);
                        q.Title = Utility.GetTitle(node);
                        q.Topic = Utility.GetTopic(node);
                        //q.ViewCount = GetViewCount(node);

                        bool contain = newIds.Contains(q.Id) || oldIds.Contains(q.Id);

                        if (!contain)
                        {                         
                            db.QuestionSet.AddObject(q);
                            newIds.Add(q.Id);
                            
                            Application.DoEvents();
                        }
                        txtTitle.Text = q.Title;
                    }

                    db.SaveChanges();
   
                }
                
            }
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.Connection.Close();
            db.Dispose();
        }

       
    }
}
