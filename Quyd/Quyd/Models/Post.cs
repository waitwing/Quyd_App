﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;
using Quyd.Resources;

namespace Quyd.Models
{
    public enum postStatus { inProgress, close };

    public class PostList
    {

        public List<Post> posts { get; private set; }

        public PostList()
        {
            posts = new List<Post>();
        }

        public async Task loadUserPostAsync(ParseUser user)
        {
            var query = from post in ParseObject.GetQuery("Post").Include("postBy")
                        where post.Get<ParseUser>("postBy") == user
                        orderby post.CreatedAt descending
                        select post;
            try
            {
                IEnumerable<ParseObject> posts_t = await query.FindAsync();
                foreach (ParseObject post in posts_t)
                {
                    posts.Add(new Post(post));
                }
            }
            catch (ParseException ex)
            {
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no post found
                }
                else
                {
                    throw ex;
                }
            }
        }

        public async Task loadStorePostAsync(Store store)
        {
            var query = from bid in ParseObject.GetQuery("Bid").Include("post").Include("post.postBy")
                        where bid.Get<ParseObject>("bidStore") == store.Object
                        orderby bid.CreatedAt descending
                        select bid;
            try
            {
                IEnumerable<ParseObject> bids_t = await query.FindAsync();
                foreach (ParseObject bid in bids_t)
                {
                    ParseObject post = bid.Get<ParseObject>("post");
                    posts.Add(new Post(post));
                }
            }
            catch (ParseException ex)
            {
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no post found
                }
                else
                {
                    throw ex;
                }
            }
        }

        public async Task loadFeedAsync(ParseUser user)
        {
            var query = from post in ParseObject.GetQuery("Post").Include("postBy")
                        where post.Get<ParseUser>("postBy") != user
                        orderby post.CreatedAt descending
                        select post;
            try
            {
                IEnumerable<ParseObject> posts_t = await query.FindAsync();
                foreach (ParseObject post_t in posts_t)
                {
                    Post post = new Post(post_t);
                    if(await post.isBidable(user) == true)
                    {
                        posts.Add(post);
                    }
                }
            }
            catch (ParseException ex)
            {
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no post found
                }
                else
                {
                    throw ex;
                }
            }
        }

        public int Size
        {
            get
            {
                return posts.Count;
            }
        }
    }

    public class Post
    {
        public ParseObject post { get; private set; }

        private ItemList postItems = null;

        private BidList bidList = null;

        public Post()
        {
            this.post = null;
            this.postItems = null;
            this.bidList = null;
        }

        public Post(ParseObject post)
        {
            this.post = post;
            this.postItems = null;
            this.bidList = null;
        }

        public Post(ParseObject post, ItemList postItems)
        {
            this.post = post;
            this.postItems = postItems;
            this.bidList = null;
        }

        public Post(ParseGeoPoint location, ParseUser user)
        {
            post = new ParseObject("Post");
            post["location"] = location;
            post["postBy"] = user;
        }

        public async Task loadAsync(string postId)
        {
            var query = from post in ParseObject.GetQuery("Post").Include("selectedStore")
                        where post.ObjectId == postId
                        select post;
            try
            {
                post = await query.FirstAsync();
                await postItems.loadPostItemsAsync(new Post(post));
            }
            catch (ParseException ex)
            {
                //not handler ""no more space exception
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no older notification found
                    post = null;
                }
            }
        }

        public async Task saveAsync()
        {
            await post.SaveAsync();
        }

        #region get set

        public ParseObject Object
        {
            get
            {
                return post;
            }
        }

        public postStatus Status
        {
            get
            {
                return post.Get<postStatus>("status");
            }
            set
            {
                post["status"] = value;
            }
        }

        public string Description
        {
            get
            {
                return post.Get<string>("description") == null ? "" : post.Get<string>("description");
            }
            set
            {
                post["description"] = value;
            }
        }

        public ParseGeoPoint Location
        {
            get
            {
                return post.Get<ParseGeoPoint>("location");
            }
            set
            {
                post["location"] = value;
            }
        }

        public ParseUser PostBy
        {
            get
            {
                return post.Get<ParseUser>("postBy");
            }
            set
            {
                post["postBy"] = value;
            }
        }

        public Store SelectedStore
        {
            get
            {
                return new Store(post.Get<ParseObject>("selectedStore"));
            }
            set
            {
                post["selectedStore"] = value;
            }
        }

        public DateTime? CreateAt
        {
            get
            {
                return post.CreatedAt;
            }
        }

        public async Task<ItemList> getUserItem()
        {
            try
            {
                if (postItems == null)
                {
                    postItems = new ItemList();
                    await postItems.loadPostItemsAsync(new Post(post));
                }
            }
            catch (ParseException ex)
            {
                //not handler ""no more space exception
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no older notification found
                }
            }

            return postItems;
        }

        public async Task<BidList> getBidList()
        {
            try
            {
                if (bidList == null)
                {
                    bidList = new BidList();
                    await bidList.getBidListAsync(new Post(post));
                }
            }
            catch (ParseException ex)
            {
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    //no post found
                }
                else
                {
                    throw ex;
                }
            }

            return bidList;
        }

        public async Task<bool> isBidable(ParseUser user)
        {
            if (PostBy.ObjectId == user.ObjectId)
            {
                return false;
            }
            else
            {
                await getBidList();
                // if no bidder
                if (bidList.Size != 0)
                {
                    if (await bidList.contain(user))
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        #endregion

    }
}
