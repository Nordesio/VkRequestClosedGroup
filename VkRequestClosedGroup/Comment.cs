using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkRequestClosedGroup
{
    public class Comment
    {
        public string id { get; set; }

        /// <summary>
        /// id поста, к которому был написан коммент
        /// </summary>
        public string post { get; set; }

        public string text { get; set; }

        public string image_links { get; set; }

        public string author { get; set; }

        public string author_id { get; set; }

        public string author_url { get; set; }

        public string owner_name { get; set; }

        /// <summary>
        /// id пользователя кому комментарий написан
        /// </summary>
        public string user_thread_id { get; set; }

        /// <summary>
        /// имя группы
        /// </summary>
        public string owner_url { get; set; }

        /// <summary>
        /// id группы вк
        /// </summary>
        public string owner_url_id { get; set; }
        public DateTime date { get; set; }

        public int likes { get; set; }

        /// <summary>
        /// Дата скачивания коммента
        /// </summary>
        public DateTime retrieve_date { get; set; }

        /// <summary>
        /// Источник (url сйта: vk.com, lenta.ru)
        /// </summary>
        public string source { get; set; }

        // <summary>
        /// ответ на комментарий или комментарий
        /// </summary>
        public string type_name { get; set; }

        // <summary>
        /// регион
        /// </summary>
        public int region { get; set; }

        // <summary>
        /// регион
        /// </summary>
        public string tags { get; set; }

        /// <summary>
        /// Фотография пользователя, оставившего комментарий
        /// </summary>
        public string photo { get; set; }
    }
}
