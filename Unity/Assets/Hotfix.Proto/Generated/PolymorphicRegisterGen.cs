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
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqBagInfo>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ResBagInfo>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqComposePet>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ResComposePet>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqUseItem>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqSellItem>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ResItemChange>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqHeartBeat>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.RespHeartBeat>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.UserInfo>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.PhoneNumber>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.Person>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.AddressBook>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.ReqLogin>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.RespLogin>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.RespErrorCode>();
			PolymorphicTypeMapper.Register<Hotfix.Proto.Proto.RespPrompt>();
        }
	}
}
