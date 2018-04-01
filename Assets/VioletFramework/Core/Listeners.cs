
//普通委托
public delegate void Listener();
public delegate void Listener<T>(T arg1);
public delegate void Listener<T, U>(T arg1, U arg2);
public delegate void Listener<T, U, V>(T arg1, U arg2, V arg3);
public delegate void Listener<T, U, V, W>(T arg1, U arg2, V arg3, W arg4);
public delegate void Listener<T, U, V, W, X>(T arg1, U arg2, V arg3, W arg4, X arg5);
public delegate void Listener<T, U, V, W, X, Y>(T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6);
public delegate void Listener<T, U, V, W, X, Y, Z>(T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6, Z arg7);

//带返回参数的委托
public delegate TR ArgsListener<TR>();
public delegate TR ArgsListener<T, TR>(T arg1);
public delegate TR ArgsListener<T, U, TR>(T arg1, U arg2);
public delegate TR ArgsListener<T, U, V, TR>(T arg1, U arg2, V arg3);
public delegate TR ArgsListener<T, U, V, W, TR>(T arg1, U arg2, V arg3, W arg4);
public delegate TR ArgsListener<T, U, V, W, X, TR>(T arg1, U arg2, V arg3, W arg4, X arg5);
public delegate TR ArgsListener<T, U, V, W, X, Y, TR>(T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6);
public delegate TR ArgsListener<T, U, V, W, X, Y, Z, TR>(T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6, Z arg7);