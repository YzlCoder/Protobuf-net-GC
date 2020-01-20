﻿using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ProtoSerializeWrapperGen
{
    #region 代码模板
	
    private const string GEN_WRAPPER_PATH = "/Scripts/Protobuf-wrapper/Generate/";

	private const string REG_CODE_TEMP = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------   
public partial class ProtoSerialize
{
    static ProtoSerialize()
    {
        $RegItem$
    }
}";

	private const string REG_CODE_ITEM = @"
		ProtoSerializeReg<$ValueType$>.Invoke = Serialize_pb_$ValueName$.Deserialize;";

	private const string REG_CODE_FILE = "ProtoSerialize.pbwrap.cs";
	
	private const string FILE_NAME_TEMP = "$TypeName$.pbwrap.cs";

	private const string CLASS_TEMP = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------   
using ProtoBuf;
using System.Collections.Generic;

public static class Serialize_pb_$TypeName$ {

$ClassContent$

}";

	
	private const string DESERIALIZE_FUNC_TEMP = @"	public static $Type$ Deserialize(ProtoReader source, $Type$ obj)
    {
		int fieldNumber;
		while ((fieldNumber = source.ReadFieldHeader()) > 0)
		{
			switch (fieldNumber)
			{
$CaseContent$
				default:
				{
					source.SkipField();
					break;
				}
			}
		}
$CreateInstance$
		return obj;
    }";

	private const string INSTANCE_TEMP = @"		obj = ProtoSerializeHelper<$Type$>.CreateInstance(obj);";
	
	private const string EMPTY_CASE_TEMP = @"
				case $CaseNum$:
				{
					break;
				}";

	private const string WRITE_BYFIELD_TEMP = @"obj.$MemberName$ = $Value$;";

	private const string WRITE_BYFUN_TEMP = @"obj.$MemberName$($Value$);";

	private const string VALUE_TEMP = "($ValueType$)(value)";
	
	private const string CASE_TEMP = @"
				case $CaseNum$:
				{
$CaseHandle$
					$WriteField$
					break;
				}";
	

	private const string CREATE_OBJ =

@"					obj = ProtoSerializeHelper<$ObjValue$>.CreateInstance(obj);
";
	
	private const string SUBITEM_TEMP = 
@"					SubItemToken token = ProtoReader.StartSubItem(source);
					var $VarName$ = Serialize_pb_$MemberType$.Deserialize(source, $MemberObj$);
					ProtoReader.EndSubItem(token, source);";

	private const string DYNAMIC_TEMP = @"
				case $CaseNum$:
				{
					SubItemToken token = ProtoReader.StartSubItem(source);
					obj = Serialize_pb_$DerivedName$.Deserialize(source, obj as $DerivedType$);
					ProtoReader.EndSubItem(token, source);
					break;
				}";
	
	private const string LIST_TEMP = 
@"					var value = obj.$MemberName$;
  					if (value == null)
  					{
  						value = new List<$SubItemType$>();
  					}
  					value.Clear();
  					do
  					{
$SubItem$
  						value.Add(($SubItemType$)subValue);
  					} while (source.TryReadFieldHeader($CaseNum$));
  ";
	private const string ARRAY_TEMP = 
@"					var list = ProtoSerializeHelper<$SubItemType$>.GetListCache();
					do
					{
$SubItem$
						list.Add(($SubItemType$)subValue);
					} while (source.TryReadFieldHeader($CaseNum$));
					var value = list.ToArray();
";
	private const string HASHSET_TEMP = 
@"					var value = obj.$MemberName$;
  					if (value == null)
  					{
  						value = new HashSet<$SubItemType$>();
  					}
  					value.Clear();
  					do
  					{
$SubItem$
  						value.Add(($SubItemType$)subValue);
  					} while (source.TryReadFieldHeader($CaseNum$));
  ";
	private const string BYTES_TEMP = 
@"					var value = obj.$MemberName$;
					value = ProtoReader.AppendBytes(value, source);";
	#endregion
	
	
	private static List<Type> allExportType = new List<Type>();

	[MenuItem("PB/生成PB-Wrap代码")]
	public static void ExportAllProtoWrapperScript()
	{
		allExportType.Clear();
		
		Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < allAssemblies.Length; i++)
		{
			Type[] types = allAssemblies[i].GetTypes();

			for (int j = 0; j < types.Length; ++j)
			{
				if (ExportProtoWrapperScript(types[j]))
				{
					allExportType.Add(types[j]);
				}
			}
		}
		
		//导出注册文件
		ExportRegistScript();
		
		AssetDatabase.Refresh();
		//ExportProtoWrapperScript(typeof(FsmVariables));
	}
	
	
    public static bool ExportProtoWrapperScript(Type type)
    {
        //没有PB标签的类型不导出
        if(type.GetAttribute<ProtoContractAttribute>() == null ||
           type.IsEnum)
        {
            return false;
        }
        
        //1. 文件名字
	    string fileName = GenScriptWrapperFileName(type);
     
        //2. class代码
        string classCode = GenScriptWrapperClassScript(type);
        
        //3.deserialize代码
        string deserializeCode = GenDeserializeScript(type);
        
	    //4.整个代码文件
	    string allCode = classCode.Replace("$ClassContent$", deserializeCode);
	    
	    System.IO.File.WriteAllText(Application.dataPath + GEN_WRAPPER_PATH + fileName, allCode);

	    return true;
    }

	private static void ExportRegistScript()
	{
		string allCode = REG_CODE_TEMP;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < allExportType.Count; ++i)
		{
			if (allExportType[i].IsAbstract)
			{
				continue;
			}
			string itemCode = REG_CODE_ITEM;
			itemCode = itemCode.Replace("$ValueType$", GetTypeFullName(allExportType[i]));
			itemCode = itemCode.Replace("$ValueName$", allExportType[i].Name);
			stringBuilder.Append(itemCode);
		}

		allCode = allCode.Replace("$RegItem$", stringBuilder.ToString());
		
		System.IO.File.WriteAllText(Application.dataPath + GEN_WRAPPER_PATH + REG_CODE_FILE, allCode);
	}
	
    private static string GenScriptWrapperFileName(Type type)
    {
        return FILE_NAME_TEMP.Replace("$TypeName$", type.Name);
    }

    private static string GenScriptWrapperClassScript(Type type)
    {
        return CLASS_TEMP.Replace("$TypeName$", type.Name);
    }

    private static string GenDeserializeScript(Type type)
    {
	    string deserCode = DESERIALIZE_FUNC_TEMP;
       
	    deserCode = deserCode.Replace("$CreateInstance$", type.IsAbstract ?  "" : INSTANCE_TEMP);
	    deserCode = deserCode.Replace("$Type$", GetTypeFullName(type));

	    StringBuilder stringBuilder = new StringBuilder();
	    
	    /* 多态 */
	    ProtoIncludeAttribute[] includes = type.GetAttributes<ProtoIncludeAttribute>();
	    foreach (var include in includes)
	    {
		    string includeCode = DYNAMIC_TEMP;
		    includeCode = includeCode.Replace("$CaseNum$", include.Tag.ToString());
		    includeCode = includeCode.Replace("$DerivedName$", include.KnownType.Name);
		    includeCode = includeCode.Replace("$DerivedType$", GetTypeFullName(include.KnownType));

		    stringBuilder.Append(includeCode);
	    }
	    
	    
	    /* 成员 */
        MemberInfo[] memberInfos = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
	    foreach (MemberInfo memberInfo in memberInfos)
	    {
		    if (memberInfo.DeclaringType != type) continue;
		    if (memberInfo.IsDefined(typeof(ProtoIgnoreAttribute), true)) continue;
		    
		    ProtoMemberAttribute memberAttr = memberInfo.GetAttribute<ProtoMemberAttribute>();
		    if (memberAttr == null)
		    {
			    continue;
		    }

		    PropertyInfo propertyInfo;
		    FieldInfo fieldInfo;
		    
		    Type memberType = null;

		    if ((propertyInfo = memberInfo as PropertyInfo) != null)
		    {
			    memberType = propertyInfo.PropertyType;
		    }
		    else if ((fieldInfo = memberInfo as FieldInfo) != null)
		    {
			    memberType = fieldInfo.FieldType;
		    }

		    if (memberType == null)
		    {
			    continue;
		    }
		    
		    string caseCode = CASE_TEMP;
		    caseCode = caseCode.Replace("$CaseNum$", memberAttr.Tag.ToString());

		    ProtoSupportSetAttribute supportSetAttribute = memberInfo.GetAttribute<ProtoSupportSetAttribute>();
		    if (supportSetAttribute != null)
		    {
			    caseCode = caseCode.Replace("$WriteField$", WRITE_BYFUN_TEMP);
			    caseCode = caseCode.Replace("$Value$", supportSetAttribute.FunctionParam);
			    caseCode = caseCode.Replace("$MemberName$", supportSetAttribute.FunctionName);
			    caseCode = caseCode.Replace("$Param$", VALUE_TEMP);
			    caseCode = caseCode.Replace("$ValueType$", GetTypeFullName(memberType));    
		    }
		    else
		    {
			    caseCode = caseCode.Replace("$WriteField$", WRITE_BYFIELD_TEMP);
			    caseCode = caseCode.Replace("$Value$", VALUE_TEMP);
			    caseCode = caseCode.Replace("$MemberName$", memberInfo.Name);
			    caseCode = caseCode.Replace("$ValueType$", GetTypeFullName(memberType));
		    }    
			    
		    
		    
		    string handleCode = GenTypeReadCode(memberInfo, memberType, memberInfo.Name, memberAttr.Tag);
		    if (!string.IsNullOrEmpty(handleCode))
		    {
			    if (!type.IsValueType && !type.IsAbstract)
			    {
				    handleCode = CREATE_OBJ + handleCode;
				    handleCode = handleCode.Replace("$ObjValue$", GetTypeFullName(type));
			    }
			    
			    stringBuilder.Append(caseCode.Replace("$CaseHandle$", handleCode));
		    }
		    
	    }
	    return deserCode.Replace("$CaseContent$", stringBuilder.ToString());
    }

	private static string GetTypeFullName(Type type)
	{
		if (type.IsArray)
		{
			Debug.Assert(!(type.GetElementType().IsArray || type.GetElementType().IsGenericType));
			return GetTypeFullName(type.GetElementType()) + "[]";
		}
		else if (type.IsGenericType)
		{
			if (type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type subType = type.GetGenericArguments()[0];
				Debug.Assert(!(subType.IsArray || subType.IsGenericType));
				return "List<" + GetTypeFullName(subType) + ">";
			}
			else if (type.GetGenericTypeDefinition() == typeof(HashSet<>))
			{
				Type subType = type.GetGenericArguments()[0];
				Debug.Assert(!(subType.IsArray || subType.IsGenericType));
				return "HashSet<" + GetTypeFullName(subType) + ">";
			}
			return type.FullName.Replace('+', '.');
		}
		return type.FullName.Replace('+', '.');
	}
	
	private static string GenTypeReadCode(MemberInfo memberInfo, Type type, string fieldName, int fieldNumber)
	{
		string getCode = fieldName;
		ProtoSupportGetAttribute supportGetAttribute = memberInfo.GetAttribute<ProtoSupportGetAttribute>();
		if (supportGetAttribute != null)
		{
			getCode = string.Format("{0}({1})", supportGetAttribute.FunctionName, supportGetAttribute.FunctionParam);
		}
		
		if (type.IsArray)
		{
			if (type == typeof(byte[]))
			{
				string bytesCode = BYTES_TEMP;
				bytesCode = bytesCode.Replace("$MemberName$", getCode);
				return bytesCode;
			}
			else
			{
				string arrayCode = ARRAY_TEMP;
				arrayCode = arrayCode.Replace("$CaseNum$", fieldNumber.ToString());

				Type subItemType = type.GetElementType();
				arrayCode = arrayCode.Replace("$SubItemType$", GetTypeFullName(subItemType));

				if (subItemType.IsArray)
				{
					//TODO error
				}
				else if (subItemType.IsGenericType)
				{
					//TODO error
				}
				else
				{
					ProtoTypeCode typecode = Helpers.GetTypeCode(subItemType);
					if (typecode != ProtoTypeCode.Unknown)
					{
						string subItemCode = "\t" + GenBaseTypeReadCode(typecode, "subValue");
						return arrayCode.Replace("$SubItem$", subItemCode);
					}
					else if(subItemType.GetAttribute<ProtoContractAttribute>() != null)
					{
						string subItemCode = SUBITEM_TEMP;
						subItemType = GetBaseType(subItemType);
						subItemCode = subItemCode.Replace("$MemberType$", subItemType.Name);
						subItemCode = subItemCode.Replace("$MemberObj$", subItemType.IsValueType ? "new " + GetTypeFullName(subItemType) + "()" : "null");
						subItemCode = subItemCode.Replace("$VarName$", "subValue");
						subItemCode = "\t" + subItemCode.Replace("\n", "\n\t");
						return arrayCode.Replace("$SubItem$", subItemCode);
					}
				}  
			}
			
		}
		else if (type.IsGenericType)
		{
			string code = "";

			if (type.GetGenericTypeDefinition() == typeof(List<>))
			{
				code = LIST_TEMP;
			}
			else
			{
				code = HASHSET_TEMP;
			}

			if (!string.IsNullOrEmpty(code))
			{
				code = code.Replace("$CaseNum$", fieldNumber.ToString());
				code = code.Replace("$MemberName$", getCode);
				Type subItemType = type.GetGenericArguments()[0];
				code = code.Replace("$SubItemType$", GetTypeFullName(subItemType));
				
				if (subItemType.IsArray)
				{
					//TODO error
				}
				else if (subItemType.IsGenericType)
				{
					//TODO error
				}
				else
				{
					ProtoTypeCode typecode = Helpers.GetTypeCode(subItemType);
					if (typecode != ProtoTypeCode.Unknown)
					{
						string subItemCode = "\t" + GenBaseTypeReadCode(typecode, "subValue");
						return code.Replace("$SubItem$", subItemCode);
					}
					else if(subItemType.GetAttribute<ProtoContractAttribute>() != null)
					{
						string subItemCode = SUBITEM_TEMP;
						subItemType = GetBaseType(subItemType);
						subItemCode = subItemCode.Replace("$MemberType$", subItemType.Name);
						subItemCode = subItemCode.Replace("$MemberObj$", subItemType.IsValueType ? "new " + GetTypeFullName(subItemType) + "()" : "null");
						subItemCode = subItemCode.Replace("$VarName$", "subValue");
						subItemCode = "\t" + subItemCode.Replace("\n", "\n\t");
						return code.Replace("$SubItem$", subItemCode);
					}
				}
			}
			else
			{
				//TODO error
			}
		}
		else
		{
			ProtoTypeCode typecode = Helpers.GetTypeCode(type);
			if (typecode != ProtoTypeCode.Unknown)
			{
				return GenBaseTypeReadCode(typecode, "value");
			}
			else if(type.GetAttribute<ProtoContractAttribute>() != null)
			{
				string subItemCode = SUBITEM_TEMP;
				type = GetBaseType(type);
				subItemCode = subItemCode.Replace("$MemberType$", type.Name);
				
				subItemCode = subItemCode.Replace("$MemberObj$", type.IsValueType ? "new " + GetTypeFullName(type) + "()" : "obj." + fieldName);
				subItemCode = subItemCode.Replace("$VarName$", "value");
				return subItemCode;
			}
		}

		return "";
	}

	private static Type GetBaseType(Type type)
	{
		Type baseType = type.BaseType;
		while (baseType.GetAttribute<ProtoContractAttribute>() != null)
		{
			ProtoIncludeAttribute[] includes = type.BaseType.GetAttributes<ProtoIncludeAttribute>();
			foreach (var include in includes)
			{
				if (include.KnownType == type)
				{
					return baseType;
				}
			}
			baseType = baseType.BaseType;
		}
		return type;
	}
	

	private static string GenBaseTypeReadCode(ProtoTypeCode typecode, string varName)
	{
		string userVar = "					var " + varName;
		switch (typecode)
		{
			case ProtoTypeCode.Int16: return userVar + " = source.ReadInt16();";
			case ProtoTypeCode.Int32: return userVar + " = source.ReadInt32();";
			case ProtoTypeCode.Int64: return userVar + " = source.ReadInt64();";
			case ProtoTypeCode.UInt16: return userVar + " = source.ReadUInt16();";
			case ProtoTypeCode.UInt32: return userVar + " = source.ReadUInt32();";
			case ProtoTypeCode.UInt64: return userVar + " = source.ReadUInt64();";
			case ProtoTypeCode.Boolean: return userVar + " = source.ReadBoolean();";
			case ProtoTypeCode.SByte: return userVar + " = source.ReadSByte();";
			case ProtoTypeCode.Byte: return userVar + " = source.ReadByte();";
			case ProtoTypeCode.Char: return userVar + " = source.ReadUInt16();";
			case ProtoTypeCode.Double: return userVar + " = source.ReadDouble();";
			case ProtoTypeCode.Single: return userVar + " = source.ReadSingle();";
			case ProtoTypeCode.DateTime: return userVar + " = source.ReadDateTime();";
			case ProtoTypeCode.Decimal: return userVar + " = source.ReadDecimal();";
			case ProtoTypeCode.String: return userVar + " = source.ReadString();";
			case ProtoTypeCode.ByteArray: return userVar + " = ProtoReader.AppendBytes(null, source)";
			case ProtoTypeCode.TimeSpan: return userVar + " = BclHelpers.ReadTimeSpan(source);";
			case ProtoTypeCode.Guid: return userVar + " = BclHelpers.ReadGuid(source);";
			case ProtoTypeCode.Uri: return userVar + " = source.ReadString();";
		}
		return "";
	}

	private static T GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
	{
		T[] attrs = memberInfo.GetCustomAttributes(typeof(T), false) as T[];
		if (attrs == null || attrs.Length == 0)
		{
			return null;
		}
		return attrs[0];
	}
	
	private static T GetAttribute<T>(this Type type) where T : Attribute
	{
		T[] attrs = type.GetCustomAttributes(typeof(T), false) as T[];
		if (attrs == null || attrs.Length == 0)
		{
			return null;
		}
		return attrs[0];
	}
	
	private static T[] GetAttributes<T>(this Type type) where T : Attribute
	{
		T[] attrs = type.GetCustomAttributes(typeof(T), false) as T[];
		return attrs;
	}
}