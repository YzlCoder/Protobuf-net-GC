using System;

/*
 * 用于提供给ProtoSerialize生成反序列化代码的标签
 * 提供Set方法用于赋值非public变量
 * FunctionName：方法名
 * FunctionParam : 参数列表字符串, 必须包含$Param$,这个通配符同来生成时替换真正的赋值参数。
 * 比如方法是两个参数的(XX,YY)。如果将赋值的当做第一个参数传入则，参数列表字符串为：$Param$, 1。(注意，其他参数必须给默认值，比如这里给了1)
 */
public class ProtoSupportSetAttribute : Attribute
{

    public string FunctionName = "";

    public string FunctionParam = "$Param$";

    public ProtoSupportSetAttribute(string functionName)
    {
        this.FunctionName = functionName;
    }
}