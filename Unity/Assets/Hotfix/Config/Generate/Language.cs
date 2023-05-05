using System;
using System.Collections.Generic;
//using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace Hotfix.Config
{
    [ProtoContract]
    //[Config]
    public partial class LanguageCategory //: ConfigSingleton<LanguageCategory>, IMerge
    {
        //[ProtoIgnore]
        //[BsonIgnore]
        private Dictionary<int, Language> dict = new Dictionary<int, Language>();
		
        //[BsonElement]
        [ProtoMember(1)]
        private List<Language> list = new List<Language>();
		/**
        public void Merge(object o)
        {
            LanguageCategory s = o as LanguageCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (Language config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }*/
		
        public Language Get(int id)
        {
            this.dict.TryGetValue(id, out var item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof(Language)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }
        
        public List<Language> GetList()
        {
            return this.list;
        }

        public Dictionary<int, Language> GetAll()
        {
            return this.dict;
        }

        public Language GetOne()
        {
            if (this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Language
	{

		/// <summary>
		/// Id
		/// </summary>
		[ProtoMember(1)]
		public int Id { get; set; }

		/// <summary>
		/// 简体中文
		/// </summary>
		[ProtoMember(2)]
		public string Chinese { get; set; }

		/// <summary>
		/// 繁体中文
		/// </summary>
		[ProtoMember(3)]
		public string ChineseTraditional { get; set; }

		/// <summary>
		/// 英语
		/// </summary>
		[ProtoMember(4)]
		public string English { get; set; }

	}
}
