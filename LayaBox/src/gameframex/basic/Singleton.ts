export default function Singleton<T>() {
    class SingletonT {
        protected constructor() { }
        private static _instance: SingletonT = null;
        /**
         * 单例对象
         */
        public static get instance(): T {
            if (SingletonT._instance == null) {
                SingletonT._instance = new this();
            }
            return SingletonT._instance as T;
        }
    }
    return SingletonT;
}