using GameFrameX.Network;
using UnityEngine;

namespace GameFrameX.Runtime
{
    public static class PolymorphicRegister
    {
        static bool serializerRegistered = false;

        private static void Init()
        {
            if (!serializerRegistered)
            {
                // StaticCompositeResolver.Instance.Register(MessagePack.Resolvers.GeneratedResolver.Instance
                // );

                // var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

                
                PolymorphicTypeMapper.Register<MessageObject>();
                PolymorphicResolver.Instance.Init();
                // MessagePackSerializer.DefaultOptions = option;
                serializerRegistered = true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            Init();
        }
    }
}