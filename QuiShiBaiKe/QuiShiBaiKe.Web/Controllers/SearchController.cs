using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PanGu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuiShiBaiKe.Web.Controllers
{
    public class SearchController : Controller
    {


        public ActionResult Search(int? pageIndex)//处理搜索的 关键词 ，搜索分页的关键词
        {
            //last: 添加分页组件
            //string strPageNum = Request["pageIndex"]; //配置了路由不能这样获取，参数
            //string strPageNum = fc["pageIndex"];

            int pagenum = 1; //当前页码
            if (pageIndex != null)
            {
                pagenum = (int)pageIndex;
            }
            string kw = Request["word"];
            if (kw == null)
            {
                kw = (string)Session["kw"];//如果，点击下一页时候，kw（关键词是null），就从session中获取；
            }
            Session["kw"] = kw;


            //导入 Lucene.dll Pangu.dll Pangu.Lucene.Analyze.dll 和 Lucene盘古分词需要的字典Dict


            string indexPath = "G:/Index";//硬盘中索引库地址

            //1. 实例化Lucene 的搜索功能类
            PhraseQuery query = new PhraseQuery();

            //2. 使用盘古分词算法类，将用户输入的关键词进行分词，
            Segment segment = new Segment();
            ICollection<WordInfo> wordInfos = segment.DoSegment(kw);

            //3. 遍历使用盘古分词分好的结果集
            foreach (var wordInfo in wordInfos)
            {
                query.Add(new Term("Msg", wordInfo.Word));//在这个索引中进行搜索 关键词 //调用Add方法添加关键词，query.Add(new Term("字段名", 关键词))，
            }

            query.SetSlop(5); // 设置两个关键词之间的距离

            //4. 将搜索结果 存到 声明的对象中
            List<SearcheResult> resultSearches = new List<SearcheResult>();
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory()); //打开索引库
            IndexReader reader = IndexReader.Open(directory, true); //实例化索引 读对象
            IndexSearcher searcher = new IndexSearcher(reader); //实例化搜索对象

            query.SetSlop(5); //关键词之间的距离
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true); //创建一个Collector，1000表示最多结果条数，Collector就是一个结果收集器。
            searcher.Search(query, null, collector); //用来搜索，Query是查询条件， filter目前传递null， results是检索结果
            //ScoreDoc[] docs = collector.TopDocs(0, collector.GetTotalHits()).scoreDocs;

            //Lucene 中的分页功能
            int pageSize = 3;
            //startIndex  Howmany
            ScoreDoc[] docs = collector.TopDocs((pagenum - 1) * pageSize, pageSize).scoreDocs; //本页显示3条搜索的数据
            //1. ScoreDoc 是查询的文档结果的数据。
            for (int i = 0; i < docs.Length; i++)
            {
                int docId = docs[i].doc;//2. ScoreDoc.doc 是获得Lucene 为每个Document（记录）分配的主键
                Document doc = searcher.Doc(docId);//3. 需要根据docId再去查询文档的详细信息记录Document，节省内存

                long id = Convert.ToInt64(doc.Get("Id"));//获得这个详细记录的id
                string msg = doc.Get("Msg");//获得这条糗事的内容

                SearcheResult result = new SearcheResult();
                result.Id = id;
                result.Url = "/Message/Index/" + id;
                result.Msg = HighLight(kw, msg);//关键词高亮显示处理
                resultSearches.Add(result);
            }

            ViewBag.totalCount = collector.GetTotalHits();
            ViewBag.pageSize = pageSize;//每页显示多少条
            ViewBag.pageIndex = pagenum;//当前页码
            ViewBag.resultSearches = resultSearches;
            ViewBag.kw = kw;
            //keyword = kw,//加载页面的时候把关键词显示出来
            return View();
        }

        private static String HighLight(string keyword, String content)
        {
            //创建HtmlFormatter，参数为高亮单词的前后缀
            PanGu.HighLight.SimpleHTMLFormatter formatter = new PanGu.HighLight.SimpleHTMLFormatter("<span class='keywordHighLight'>", "</span>");
            //创建HighLighter ,输入HtmlFormater 和 盘古分词对象 Semgent
            PanGu.HighLight.Highlighter highlighter = new PanGu.HighLight.Highlighter(formatter, new Segment());

            highlighter.FragmentSize = 100;//设置每个摘要字段的显示字符数
            return highlighter.GetBestFragment(keyword, content);//获取最匹配的摘要字段

        }

        //把Lucene 的查询结果显示到界面中
        public class SearcheResult
        {
            public long Id { get; set; }
            public string Url { get; set; } //用来存糗事的id
            public string Msg { get; set; } //用来存糗事的内容
        }
    }
}
