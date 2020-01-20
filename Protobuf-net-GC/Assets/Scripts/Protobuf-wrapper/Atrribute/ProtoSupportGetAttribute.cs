using System;

/*
 * 用于提供给ProtoSerialize生成反序列化代码的标签
 * 提供Get方法用于获取非public变量
 * FunctionName：方法名
 * FunctionParam : 参数列表字符串
 */
public class ProtoSupportGetAttribute : Attribute
{

    public string FunctionName = "";

    public string FunctionParam = "";

    public ProtoSupportGetAttribute(string functionName)
    {
        this.FunctionName = functionName;
    }
}