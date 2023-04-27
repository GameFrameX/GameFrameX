using System;
using System.Collections.Generic;
//using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace Hotfix.Config
{
    [ProtoContract]
    //[Config]
    public partial class ItemCategory //: ConfigSingleton<ItemCategory>, IMerge
    {
        //[ProtoIgnore]
        //[BsonIgnore]
        private Dictionary<int, Item> dict = new Dictionary<int, Item>();
		
        //[BsonElement]
        [ProtoMember(1)]
        private List<Item> list = new List<Item>();
		/**
        public void Merge(object o)
        {
            ItemCategory s = o as ItemCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (Item config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }*/
		
        public Item Get(int id)
        {
            this.dict.TryGetValue(id, out var item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Item)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Item> GetAll()
        {
            return this.dict;
        }

        public Item GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Item
	{

		/// <summary>
		/// Id
		/// </summary>
		[ProtoMember(1)]
		public int Id { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[ProtoMember(2)]
		public string Name { get; set; }

		/// <summary>
		/// 英语
		/// </summary>
		[ProtoMember(3)]
		public string CanSell { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(4)]
		public string Show { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(5)]
		public string SellNum { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(6)]
		public string Desc { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(7)]
		public string UseType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(8)]
		public string Param { get; set; }

	}
}
