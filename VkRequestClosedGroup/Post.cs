using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkRequestClosedGroup
{
    public class Post
    {
        public Post()
        {
            links_img = new List<string>();
            links_url = new List<string>();
            retrieve_date = DateTime.Now;
        }
        //url поста
        public string id { get; set; }
        //public string task_id { get; set; }
        // https://vk.com
        public string source { get; set; }
        //url поста
        public string url { get; set; }
        //дата получения
        public DateTime retrieve_date { get; set; }
        //дата поста
        public DateTime? date { get; set; }
        
        public string name { get; set; }
        //текст
        public string text { get; set; }
        //автор, если есть пост, взятый из предложки
        public string author { get; set; }
        public string author_post_link { get; set; } //ссылка на пользователя, написавшего пост
        public string author_post_id { get; set; } //id пользователя, написавшего пост
        //public string tizer_url { get; set; }
        //public string meta_title { get; set; }
        // картинка с кружочка поста
        public string thumbnail { get; set; }
        //группа владелец
        public string owner_name { get; set; }
        //ссылка на него
        public string owner_url { get; set; }
        //id
        public string owner_id { get; set; }
        public List<string> links_img { get; set; }
        public List<string> links_url { get; set; }
        public int views { get; set; }
        public int comments { get; set; }
        /// <summary>
        /// категория новости, добытая с сайта
        /// </summary>
        public string tags { get; set; }
    }
}
