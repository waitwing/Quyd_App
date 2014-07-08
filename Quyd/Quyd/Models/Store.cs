﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parse;
using Quyd.Resources;

namespace Quyd.Models
{
    class Store
    {
        private ParseObject store;

        public ItemList items { get; private set; }

        public Store()
        {
            store = null;
        }

        public Store(ParseObject store)
        {
            this.store = store;
        }

        public Store(string name, ParseGeoPoint location, IList<string> phones, ItemList items)
        {
            store = new ParseObject("Store");
            store["name"] = name;
            var owner = ParseUser.CurrentUser;
            store["owner"] = owner;
            store["location"] = location;
            store["phones"] = phones;
        }

        public async Task saveAsync()
        {
            try
            {
                await this.validAsync();
                await store.SaveAsync();


                //edit to save items
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task loadAsync(ParseUser owner)
        {
            var query = from store_t in ParseObject.GetQuery("Store")
                        where store_t.Get<ParseUser>("owner").Equals(owner)
                        select store_t;
            try
            {
                store = await query.FirstAsync();
                items = new ItemList();
                await items.loadStoreItemAsync(new Store(store));
            }
            catch (ParseException ex)
            {
                if (ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    throw new QuydException(QuydException.ErrorCode.store_notFound, "Store not found");
                }
                else
                {
                    throw ex;
                }
            }
        }

        private async Task<bool> isNameExistAsync(string storeName)
        {
            try
            {
                var findByNameQuery = from store_t in ParseObject.GetQuery("Store")
                                      where store_t.Get<string>("name").Equals(storeName)
                                      select store_t;
                ParseObject storeObject = await findByNameQuery.FirstAsync();
                return true;
            }
            catch(ParseException ex)
            {
                if(ex.Code == ParseException.ErrorCode.ObjectNotFound)
                {
                    return false;
                }
                else
                {
                    throw ex;
                }
            }
        }

        private async Task<bool> validAsync()
        {
            //put validation here
            if(this.Name.Length==0)
            {
                throw new QuydException(QuydException.ErrorCode.store_nameTooShort, "Store name is too short.");
            }
            else if(this.Location.Equals(new ParseGeoPoint(0,0)))
            {
                throw new QuydException(QuydException.ErrorCode.store_locationInvalid, "Location is invalid.");
            }
            else if(this.Phones.Count == 0)
            {
                throw new QuydException(QuydException.ErrorCode.store_phoneInvalid, "Phone number is invalid.");
            }
            else
            {
                bool nameExist = await isNameExistAsync(store.Get<string>("name"));
                if(nameExist)
                {
                    throw new QuydException(QuydException.ErrorCode.store_nameExist, "Store name is already exist.");
                }
                else 
                {
                    return true;
                }
            }
        }

        #region Store Getter Setter

        public string Name
        {
            get
            {
                return store.Get<string>("name");
            }
            set
            {
                store["name"] = value;
            }
        }

        public ParseGeoPoint Location
        {
            get
            {
                return store.Get<ParseGeoPoint>("location");
            }
            set
            {
                store["location"] = value;
            }
        }

        public IList<string> Phones
        {
            get
            {
                return store.Get<IList<string>>("phones");
            }
            set
            {
                store["phones"] = value;
            }
        }

        public string OwnerId
        {
            get
            {
                return store.Get<string>("owner");
            }
        }

        public ParseObject Object
        {
            get
            {
                return store;
            }
        }

        #endregion

    }
}