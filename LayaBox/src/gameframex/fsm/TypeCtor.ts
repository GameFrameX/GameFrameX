/**
 * 运行时类型描述:以构造函数代替 C# 的 System.Type。
 * 泛型约束 `T extends object` 与 Unity `where T : class` 对齐。
 */
type TypeCtor<T> = new (...args: never[]) => T;

export default TypeCtor;
