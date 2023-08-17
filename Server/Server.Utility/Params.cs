namespace Server.Utility;

public abstract class Param
{
}

public class OneParam<T> : Param
{
    public readonly T Value;

    public OneParam(T t)
    {
        Value = t;
    }
}

public class TwoParam<T1, T2> : Param
{
    public T1 Value1;
    public T2 Value2;

    public TwoParam(T1 t1, T2 t2)
    {
        Value1 = t1;
        Value2 = t2;
    }
}

public class ThreeParam<T1, T2, T3> : Param
{
    public T1 Value1;
    public T2 Value2;
    public T3 Value3;

    public ThreeParam(T1 t1, T2 t2, T3 t3)
    {
        Value1 = t1;
        Value2 = t2;
        Value3 = t3;
    }
}

public class FourParam<T1, T2, T3, T4> : Param
{
    public T1 Value1;
    public T2 Value2;
    public T3 Value3;
    public T4 Value4;

    public FourParam(T1 t1, T2 t2, T3 t3, T4 t4)
    {
        Value1 = t1;
        Value2 = t2;
        Value3 = t3;
        Value4 = t4;
    }
}

public class FiveParam<T1, T2, T3, T4, T5> : Param
{
    public T1 Value1;
    public T2 Value2;
    public T3 Value3;
    public T4 Value4;
    public T5 Value5;

    public FiveParam(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        Value1 = t1;
        Value2 = t2;
        Value3 = t3;
        Value4 = t4;
        Value5 = t5;
    }
}