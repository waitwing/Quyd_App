﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;

namespace Quyd.Models
{
    public class BidList
    {
        List<Bid> bidList;

        public BidList()
        {
            bidList = null;
        }

        public BidList(List<Bid> bidList)
        {
            this.bidList = bidList;
        }

        public async Task<BidList> getBidListAsync(Post post)
        {
            if (bidList == null)
            {
                var query = from bid in ParseObject.GetQuery("Bid")
                            where bid.Get<ParseObject>("post") == post.Object
                            select bid;
                try
                {
                    IEnumerable<ParseObject> bids_t = await query.FindAsync();

                    bidList = new List<Bid>();

                    foreach(var bid in bids_t)
                    {
                        bidList.Add(new Bid(bid));
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
            return new BidList(bidList);
        }

        public async Task<bool> contain(ParseUser user)
        {
            foreach(var bid in bidList)
            {
                if((await bid.getStoreAsync()).OwnerId == user.ObjectId)
                {
                    return true;
                }
            }
            return false;
        }

        public int Size
        {
            get
            {
                return bidList.Count;
            }
        }
    }

    public class Bid
    {
        ParseObject bid;

        ItemList storeBidItems = null;

        public Bid()
        {
            bid = new ParseObject("Bid");
        }

        public Bid(ParseObject bid)
        {
            this.bid = bid;
        }

        public async Task<Store> getStoreAsync()
        {
            return new Store(await bid.Get<ParseObject>("bidStore").FetchIfNeededAsync());
        }

        public async Task<ItemList> getStoreBidItems(ItemList userItems)
        {
            if (storeBidItems == null)
            {
                var store = await getStoreAsync();
                await storeBidItems.loadStoreItemsAsync(store, bid.CreatedAt, userItems);
            }
            return storeBidItems;
        }

        public async Task saveAsync()
        {
            await bid.SaveAsync();
        }

        public Post Post 
        {
            get
            {
                return new Post(bid.Get<ParseObject>("post"));
            }

            set
            {
                bid["post"] = value.Object;
            }
        }

        public Store bidStore
        {
            get
            {
                return new Store(bid.Get<ParseObject>("bidStore"));
            }

            set
            {
                bid["bidStore"] = value.Object;
            }
        }
    }
}
