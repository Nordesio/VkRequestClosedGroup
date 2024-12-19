using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.AudioBypassService.Extensions;
using Microsoft.Extensions.DependencyInjection;
namespace VkRequestClosedGroup
{
    class Program
    {
        public static void Main()
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkApi(services);
            api.Authorize(new ApiAuthParams()
            {
                Login = "",
                Password = "",
                ApplicationId = 51597614,
                AccessToken = "vk1.a.gqeG5bhomGzFZz99PE29AodK_8BezENCUoz9U9oeKph8GpYIVoi_UqM_84gVqRAghG-qzArXllFHptWvbHvJmQn9fkWp8tmSO7QogNqALFgYWs5Oik_UE6JDWjoqbdCjK3gT00h9H1EMbuh8uaaQIE4j-wI0fJYVYDctcuY_dwW0l0zyM_04W4NUtJ0westoM6iZ3JpymBUGF_lbISXsPQ",
                Settings = Settings.All
            });
            
            
            Console.WriteLine("Введите название группы, в которую хотите вступить (или 'выйти' для выхода):");
            while (true)
            {

                var group = Console.ReadLine().Replace("https://vk.com/", "");

                if (group.ToLower().Equals("exit"))
                {
                    break;
                }
                //для тестов
                //if (group.Equals("download"))
                //{
                //    Task.Run(() => DownloadGroupPostsAndComments(api, "topor_18"));
                //}
                Task.Run(() => ProcessGroupRequest(api, group));
            }

        }

        public static async Task ProcessGroupRequest(VkApi api, string group)
        {
            try
            {
                string url = $"https://api.vk.com/method/groups.getById?access_token={api.Token}&group_id={group}&v=5.131";
                var parsed = JsonConvert.DeserializeObject<JObject>(new WebClient().DownloadString(url));
                var groupInfo = parsed["response"].First;

                int groupId = groupInfo["id"].Value<int>();
                int isClosed = groupInfo["is_closed"].Value<int>();

                Console.WriteLine($"\nId = {groupId}, Закрытая группа или нет (0 - открытая, 1 - закрытая) - {isClosed}");

                JoinGroup(api, groupId);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке группы {group}: {ex.Message}");
            }
        }

        public static async void JoinGroup(VkApi api, long groupId)
        {
            try
            {

                api.Groups.Join(groupId);
                Console.WriteLine($"Запрос на вступление в группу {groupId} отправлен успешно.");

                var currentUser = api.Users.Get(new List<long> { });
                long userId = currentUser.FirstOrDefault()?.Id ?? 0;
                bool isMember = false;
                while (!isMember)
                {

                    await Task.Delay(30000);


                    var groups = api.Groups.Get(new GroupsGetParams
                    {
                        UserId = userId,
                        Extended = false
                    });

                    if (groups.Any(g => g.Id == groupId))
                    {
                        Console.WriteLine($"Запрос принят, вы теперь участник группы {groupId}.");
                        DownloadGroupPostsAndComments(api, groupId.ToString());
                        isMember = true;
                    }
                    else
                    {
                        Console.WriteLine($"Запрос в группу {groupId} отклонен или еще не обработан. Повторная проверка через 30 секунд...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке запроса в группу {groupId}: {ex.Message}");
            }
        }

        public static async Task DownloadGroupPostsAndComments(VkApi api, string group_)
        {
            try
            {
                string url = $"https://api.vk.com/method/groups.getById?access_token={api.Token}&group_id={group_}&v=5.131";
                var parsed = JsonConvert.DeserializeObject<JObject>(new WebClient().DownloadString(url));
                var groupInfo = parsed["response"].First;

                long groupId = groupInfo["id"].Value<int>();
                Console.WriteLine($"Начинаем скачивать посты и комментарии из группы {groupId}...");

                // Получение постов из группы
                var posts = api.Wall.Get(new WallGetParams
                {
                    OwnerId = -groupId, // Для групп ID должен быть отрицательным
                    Count = 20 // Получить первые 20 постов
                });

                Console.WriteLine($"Найдено постов: {posts.WallPosts.Count}");

                List<Post> vk_posts = new List<Post>();
                List<Comment> comments = new List<Comment>();
                var group = api.Groups.GetById(new List<string> { groupId.ToString() }, null, VkNet.Enums.Filters.GroupsFields.All).FirstOrDefault();

                foreach (var post in posts.WallPosts)
                {
                    Console.WriteLine($"\nПост ID: {post.Id}, Текст: {post.Text}");
                    Post vk_post = new Post();
                    long postId = post.Id.Value;
                    long? ownerId = post.OwnerId;
                    // Генерация ссылки на пост
                    string postLink = $"https://vk.com/wall{ownerId}_{postId}";
                    vk_post.id = postLink;
                    vk_post.source = "https://vk.com";
                    vk_post.url = postLink;
                    vk_post.date = post.Date.Value;
                    vk_post.name = "Vk пост: ";
                    vk_post.text = post.Text;
                    if(post.SignerId != null)
                    {
                        var user = api.Users.Get(new List<long> { post.SignerId.Value }).FirstOrDefault();
                        vk_post.author = user.FirstName + " " + user.LastName;
                        vk_post.author_post_link = "id"+user.Id.ToString();
                        vk_post.author_post_id = "id" + user.Id.ToString();
                    }
                    //var user = api.Users.Get(new List<long> { ownerId }).FirstOrDefault();
                    //vk_post.author = post.
                    vk_post.thumbnail = group.Photo200?.ToString();
                    vk_post.owner_name = group.Name;
                    vk_post.owner_id = "https://vk.com/public" + (-ownerId);
                    vk_post.owner_url = "https://vk.com/public" + (-ownerId);
                    vk_post.views = post.Views.Count;
                    vk_post.comments = post.Comments.Count;
                    List<string> photos = new List<string>();
                    List<string> links = new List<string>();
                    if (post.Attachments != null && post.Attachments.Count > 0)
                    {
                        

                        foreach (var attachment in post.Attachments)
                        {
                            // Проверяем, является ли вложение фотографией
                            if (attachment.Instance is Photo photo)
                            {
                                string photoUrl = photo.Sizes.LastOrDefault()?.Url.ToString();
                                photos.Add(photoUrl);
                            }
                            if (attachment.Instance is Link link)
                            {
                                links.Add(link.ToString());
                            }
                        }
                    }
                    vk_post.links_img = photos;
                    vk_post.links_url = links;

                    vk_posts.Add(vk_post);

                    int offset = 0;
                    int count = 100; // Количество комментариев за один запрос
                    bool hasMoreComments = true;

                    while (hasMoreComments)
                    {
                        
                        await Task.Delay(300); 

                        // Получение комментариев с учетом смещения
                        var comments_from_post = api.Wall.GetComments(new WallGetCommentsParams
                        {
                            OwnerId = -groupId, // ID группы с отрицательным значением
                            PostId = post.Id.Value,
                            Count = count,
                            ThreadItemsCount = 10, 
                            Offset = offset
                        });


                        if (comments_from_post.Items.Count == 0)
                        {
                            hasMoreComments = false; // Если больше нет комментариев, выходим из цикла
                            break;
                        }

                        offset += count;

                        foreach (var comment in comments_from_post.Items)
                        {

                            if (comment.Thread != null && comment.Thread.Count > 0)
                            {
                                int threadOffset = 0;
                                int threadCount = 10; // Количество вложенных комментариев за раз
                                int totalThreadComments = 10;
                                bool moreThreadComments = true;

                                while (moreThreadComments)
                                {
                                    // Повторяем для получения всех вложенных комментариев
                                    var threadComments = api.Wall.GetComments(new WallGetCommentsParams
                                    {
                                        OwnerId = -groupId, // ID группы
                                        PostId = post.Id.Value,
                                        CommentId = comment.Id, // ID родительского комментария
                                        Count = threadCount,
                                        Offset = threadOffset
                                    });

                                    if (threadComments.Count < totalThreadComments)
                                    {
                                        moreThreadComments = false;
                                    }
                                    else
                                    {
                                        totalThreadComments += threadCount;
                                    }

                                    threadOffset += threadCount;

                                    foreach (var reply in threadComments.Items)
                                    {
                                        Comment com1 = new Comment();

                                        var userId1 = reply.FromId.Value;

                                        // Запрашиваем информацию о пользователе
                                        var user1 = api.Users.Get(new List<long> { userId1 }).FirstOrDefault();

                                        com1.id = $"https://vk.com/wall-{groupId}_{post.Id}?reply={reply.Id}";
                                        com1.post = postLink;
                                        com1.text = reply.Text;

                                        if (reply.Attachments != null && reply.Attachments.Count > 0)
                                        {
                                            foreach (var attachment in reply.Attachments)
                                            {

                                                if (attachment.Instance is Photo photo)
                                                {

                                                    com1.image_links = photo.Sizes.LastOrDefault()?.Url.ToString();
                                                    break;
                                                }
                                            }
                                        }
                                        if (user1 != null)
                                        {
                                            com1.author = user1.FirstName + " " + user1.LastName;
                                            com1.author_id = "id" + user1.Id.ToString();
                                            com1.author_url = "id" + (string.IsNullOrEmpty(user1.Domain) ? user1.Id.ToString() : user1.Domain);
                                        }
                                        com1.owner_name = group.Name;
                                        string pattern = @"\[id(?<id>\d+)\|[^\]]+\]";
                                        var match = Regex.Match(com1.text, pattern);

                                        if (match.Success)
                                        {
                                            com1.user_thread_id = "id" + match.Groups["id"].Value; 
                                        }
                                        com1.owner_url = "public" + (-ownerId);
                                        com1.owner_url_id = "public" + (-ownerId);
                                        com1.date = reply.Date.Value;
                                        if (reply.Likes != null)
                                            com1.likes = reply.Likes.Count;
                                        com1.retrieve_date = DateTime.Now;
                                        com1.source = "https://vk.com";
                                        com1.type_name = "Комментарий";
                                        com1.region = 0;
                                        if (user1 != null)
                                        {
                                            if (user1.PhotoMax != null)
                                                com1.photo = user1.PhotoMax.ToString();
                                        }
                                        comments.Add(com1);
                                    }
                                }
                            }


                            Comment com = new Comment();

                            var userId = comment.FromId.Value;

                            // Запрашиваем информацию о пользователе
                            var user = api.Users.Get(new List<long> { userId }).FirstOrDefault();

                            com.id = $"https://vk.com/wall-{groupId}_{post.Id}?reply={comment.Id}";
                            com.post = postLink;
                            com.text = comment.Text;
                            
                            if (comment.Attachments != null && comment.Attachments.Count > 0)
                            {
                                foreach (var attachment in comment.Attachments)
                                {
                                    
                                    if (attachment.Instance is Photo photo)
                                    {
                                        
                                        com.image_links =  photo.Sizes.LastOrDefault()?.Url.ToString();
                                        break; 
                                    }
                                }
                            }
                            if(user != null)
                            {
                                com.author = user.FirstName + " " + user.LastName;
                                com.author_id = "id"+user.Id.ToString();
                                com.author_url = "id"+(string.IsNullOrEmpty(user.Domain) ? user.Id.ToString() : user.Domain);
                            }
                            com.owner_name = group.Name;
                            if (comment.ReplyToComment.HasValue)
                            {
                                com.user_thread_id = comment.ReplyToComment.Value.ToString();
                            }
                            com.owner_url = "public" + (-ownerId);
                            com.owner_url_id = "public" + (-ownerId);
                            com.date = comment.Date.Value;
                            if(comment.Likes != null)
                            com.likes = comment.Likes.Count;
                            com.retrieve_date = DateTime.Now;
                            com.source = "https://vk.com";
                            com.type_name = "Комментарий";
                            com.region = 0;
                            if(user != null){
                                if (user.PhotoMax != null)
                                    com.photo = user.PhotoMax.ToString();
                            }
                            comments.Add(com);

                        }



                        Console.WriteLine($"Получено комментариев: {comments_from_post.Items.Count}, смещение: {offset}");

                        foreach (var comment in comments_from_post.Items)
                        {
                            Console.WriteLine($"Комментарий ID: {comment.Id}, Текст: {comment.Text}");
                        }

                        // Увеличиваем offset на количество полученных комментариев
                        offset += comments_from_post.Items.Count;

                        // Если комментариев меньше 100, значит больше страниц нет
                        if (comments_from_post.Items.Count < 100)
                        {
                            hasMoreComments = false;
                        }
                    }

                }
                Console.WriteLine("Все 20 постов выкачены");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при скачивании постов или комментариев: {ex.Message}");
            }
        }

    }

}
