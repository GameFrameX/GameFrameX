using PolymorphicMessagePack;

namespace Server.Proto.Formatter
{
	public partial class PolymorphicRegister
	{
	    static PolymorphicRegister()
        {
            System.Console.WriteLine("***PolymorphicRegister Init***"); 
            Register();
        }

		public static void Register()
        {
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqBagInfo>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ResBagInfo>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqComposePet>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ResComposePet>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqUseItem>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqSellItem>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ResItemChange>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqHeartBeat>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.RespHeartBeat>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.UserInfo>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.PhoneNumber>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.Person>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.AddressBook>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.ReqLogin>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.RespLogin>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.RespErrorCode>();
			PolymorphicTypeMapper.Register<Server.Proto.Proto.RespPrompt>();
        }
	}
}
