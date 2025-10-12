using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;


public class StaticDataTools : EditorWindow
{
    [MenuItem("Tools/ExeclToCSV")]
    public static void Execl2CSV()
    {
        ConvertExcel("../ExcelData", "Game/CSV");
    }

    [MenuItem("Tools/生成CSV解析类")]
    static void LoadCsv()
    {
        _LoadCsv();
    }

    public static void ConvertExcel(string Execl_Path, string Dest_Path, int indexOfFormat = 1)
    {
        // 编码格式
        Encoding encoding = Encoding.GetEncoding("utf-8");
        // excel目录
        var srcPath = Path.Combine(Application.dataPath, Execl_Path);
        if (!Directory.Exists(srcPath))
        {
            Debug.Log("Excel 目录不存在：" + srcPath);
            return;
        }
        // 存储位置
        var destPath = Path.Combine(Application.dataPath, Dest_Path);
        DeleteAllFile(destPath);
        //Debug.Log($"清理现有csv:{Dest_Path}");
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }
        //获取指定路径下面的所有资源文件  
        DirectoryInfo direction = new DirectoryInfo(srcPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        var fileCount = 0;
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Extension != ".xlsx") continue;
            //剔除副本
            if (files[i].Name.Contains("~$")) continue;
            //构造Excel工具类
            ExcelUtility excel = new ExcelUtility(files[i].FullName);

            //判断输出类型
            string output = Path.Combine(destPath, files[i].Name);
            if (indexOfFormat == 0)
            {
                output = output.Replace(".xlsx", ".json");
                excel.ConvertToJson(output, encoding);
            }
            else if (indexOfFormat == 1)
            {
                output = output.Replace(".xlsx", ".csv");
                excel.ConvertToCSV(output, encoding);
            }
            else if (indexOfFormat == 2)
            {
                output = output.Replace(".xlsx", ".xml");
                excel.ConvertToXml(output);
            }

            Debug.Log(string.Format("Execl转换:{0}", files[i].Name));

            fileCount++;
        }
        //刷新本地资源
        AssetDatabase.Refresh();

        Debug.Log($"Execl转换完成：{Execl_Path}，数量：{fileCount}");
    }

    /// <summary>
    /// 删除指定路径下的所有文件和文件夹
    /// </summary>
    /// <param name="folderPath">目标目录路径</param>
    public static void DeleteAllFile(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"目录不存在，无需清理: {folderPath}");
            return;
        }

        try
        {
            // 删除所有文件
            foreach (string file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
                Debug.Log($"已删除文件: {file}");
            }

            // 递归删除所有子目录
            foreach (string subDir in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(subDir, true); // true 表示递归删除
                Debug.Log($"已删除目录: {subDir}");
            }

            Debug.Log($"清理完成: {folderPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"删除文件/目录时发生错误: {e.Message}");
        }
    }

    static void _LoadCsv()
    {
        string csvPath = Application.dataPath + "\\Game\\CSV";
        if (!Directory.Exists(csvPath))
        {
            Debug.LogError("当前路径不存在" + csvPath);
            return;
        }

        DirectoryInfo root = new DirectoryInfo(csvPath);
        foreach (FileInfo f in root.GetFiles())
        {
            string fileName = f.Name;
            if (fileName.Contains(".meta")) continue;
            string filePath = csvPath + "\\" + fileName;

            List<string[]> dataList = StaticUtils.ParseCSV(filePath, ',');
            if (dataList.Count < 3)
            {
                Debug.LogError("error:表最少应该3行：" + fileName);
                return;
            }
            Debug.Log("buildClass:" + fileName);
            BuildClass(fileName, dataList[1], dataList[2]);
        }
    }

    // 解析csv表
    


    // 创建class
    static public void BuildClass(string fileName, string[] strIDs, string[] strTypes)
    {
        // 准备一个代码编译器单元
        CodeCompileUnit unit = new CodeCompileUnit();
        // 设置命名空间（这个是指要生成的类的空间）
        string nameSpace = "";
        CodeNamespace myNamespace;
        if (string.IsNullOrEmpty(nameSpace))
        {
            myNamespace = new CodeNamespace();
        }
        else
        {
            myNamespace = new CodeNamespace(nameSpace);
        }
        // 导入必要的命名空间引用
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));
        myNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

        // 去掉后缀 .csv
        fileName = fileName.Substring(0, fileName.Length - 4);
        // Code:代码体
        CodeTypeDeclaration myClass = new CodeTypeDeclaration("Static_" + fileName + "_t");
        // 指定为类
        myClass.IsClass = true;
        //设置类的访问类型
        myClass.TypeAttributes = TypeAttributes.Public; // | TypeAttributes.Sealed;
        // 把这个类放在这个命名空间下
        myNamespace.Types.Add(myClass);
        // 把该命名空间加入到编译器单元的命名空间集合中
        unit.Namespaces.Add(myNamespace);
        //添加构造方法
        CodeConstructor constructor = new CodeConstructor();
        constructor.Attributes = MemberAttributes.Public;
        //添加一个参数
        CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Dictionary<String, String>)), "rValueSet");
        param.Direction = FieldDirection.Ref;
        constructor.Parameters.Add(param);
        ///将构造方法添加到myClass类中
        myClass.Members.Add(constructor);

        string strSpace = "             ";

        for (int i = 0; i < strIDs.Length; i++)
        {
            if (string.IsNullOrEmpty(strIDs[i])) continue;
            //string strID = strSpace + "String str_" + strIDs[i] + " = rValueSet[" + "\"" + strIDs[i] + "\"" + "];";
            string strID = $"{strSpace}String str_{strIDs[i]} = rValueSet[\"{strIDs[i]}\"];";
            constructor.Statements.Add(new CodeSnippetStatement(strID));
            string strValue;

            if (strTypes[i] == "int")
            {
                //strValue = strSpace + "if (str_" + strIDs[i] + "== \"\")" + "{" + strIDs[i] + "= 0;}" + "else{ " + strIDs[i] + "= int.Parse(str_" + strIDs[i] + "); }";
                strValue = $"{strSpace}{strIDs[i]} = Utils.IntParseByString(str_{strIDs[i]});";
            }
            else if (strTypes[i] == "float")
            {
                //strValue = strSpace + "if (str_" + strIDs[i] + "== \"\")" + "{" + strIDs[i] + "= 0;}" + "else{ " + strIDs[i] + "= float.Parse(str_" + strIDs[i] + "); }";
                strValue = $"{strSpace}{strIDs[i]} = Utils.FloatParseByString(str_{strIDs[i]});";
            }
            else if (strTypes[i] == "boolean")
            {
                //strValue = strSpace + "if (str_" + strIDs[i] + ".ToUpper()== \"TRUE\")" + "{" + strIDs[i] + "= true;}" + "else{ " + strIDs[i] + "= false; }";
                strValue = $"{strSpace}{strIDs[i]} = Utils.BooleanParseByString(str_{strIDs[i]});";
            }
            else if (strTypes[i] == "long")
            {
                //strValue = strSpace + "if (str_" + strIDs[i] + "== \"\")" + "{" + strIDs[i] + "= 0;}" + "else{ " + strIDs[i] + "= decimal.ToInt64(decimal.Parse(str_" + strIDs[i] + ", System.Globalization.NumberStyles.Any));}";
                strValue = $"{strSpace}{strIDs[i]} = Utils.LongParseByString(str_{strIDs[i]});";
            }
            else if (strTypes[i] == "list<int>")
            {
                strValue = $"{strSpace}{strIDs[i]} = Utils.ParseIntStrings(str_{strIDs[i]}, ';');";
            }
            else if (strTypes[i] == "list<float>")
            {
                strValue = $"{strSpace}{strIDs[i]} = Utils.ParseFloatStrings(str_{strIDs[i]}, ';');";
            }
            else if (strTypes[i] == "list<boolean>")
            {
                strValue = $"{strSpace}{strIDs[i]} = Utils.ParseBooleanStrings(str_{strIDs[i]}, ';');";
            }
            else if (strTypes[i] == "list<long>")
            {
                strValue = $"{strSpace}{strIDs[i]} = Utils.ParseLongStrings(str_{strIDs[i]}, ';');";
            }
            else if (strTypes[i] == "list<string>")
            {
                strValue = $"{strSpace}{strIDs[i]} = Utils.ParseStrings(str_{strIDs[i]}, ';');";
            }
            else
            {
                //strValue += "str_" + strIDs[i] + ";";
                strValue = $"{strSpace}{strIDs[i]} = str_{strIDs[i]};";
            }
            constructor.Statements.Add(new CodeSnippetStatement(strValue));

            constructor.Statements.Add(new CodeSnippetStatement("\r\n"));
        }

        //添加特特性
        // myClass.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(string))));
        // 生成C#脚本("VisualBasic"：VB脚本)
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        CodeGeneratorOptions options = new CodeGeneratorOptions();
        // 代码风格:大括号的样式{}
        options.BracingStyle = "C";
        // 是否在字段、属性、方法之间添加空白行
        options.BlankLinesBetweenMembers = true;

        //添加字段
        for (int i = 0; i < strIDs.Length; i++)
        {
            if (string.IsNullOrEmpty(strIDs[i])) continue;
            CodeMemberField field;
            if (strTypes[i] == "int")
            {
                field = new CodeMemberField(typeof(int), strIDs[i]);
            }
            else if (strTypes[i] == "float")
            {
                field = new CodeMemberField(typeof(float), strIDs[i]);
            }
            else if (strTypes[i] == "boolean")
            {
                field = new CodeMemberField(typeof(bool), strIDs[i]);
            }
            else if (strTypes[i] == "long")
            {
                field = new CodeMemberField(typeof(long), strIDs[i]);
            }
            else if (strTypes[i] == "list<int>")
            {
                field = new CodeMemberField(typeof(List<int>), strIDs[i]);
            }
            else if (strTypes[i] == "list<float>")
            {
                field = new CodeMemberField(typeof(List<float>), strIDs[i]);
            }
            else if (strTypes[i] == "list<boolean>")
            {
                field = new CodeMemberField(typeof(List<bool>), strIDs[i]);
            }
            else if (strTypes[i] == "list<long>")
            {
                field = new CodeMemberField(typeof(List<long>), strIDs[i]);
            }
            else if (strTypes[i] == "list<string>")
            {
                field = new CodeMemberField(typeof(List<string>), strIDs[i]);
            }
            else
            {
                field = new CodeMemberField(typeof(string), strIDs[i]);
            }

            //设置访问类型
            field.Attributes = MemberAttributes.Public;
            //添加到myClass类中
            myClass.Members.Add(field);
        }

        /*
        //添加方法
        CodeMemberMethod method = new CodeMemberMethod();
        //方法名
        method.Name = "Static_" + fileName + "_t";
        //访问类型
        method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        //添加一个参数
        //method.Parameters.Add(new CodeParameterDeclarationExpression( typeof(Dictionary<String, String>), "rValueSet"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Message)), "rValueSet"));
        //设置返回值类型：int/不设置则为void
        //method.ReturnType = new CodeTypeReference(typeof(int));
        //设置返回值
        method.Statements.Add(new CodeSnippetStatement(" return number+1; "));
        ///将方法添加到myClass类中
        myClass.Members.Add(method);

        //添加属性
        CodeMemberProperty property = new CodeMemberProperty();
        //设置访问类型
        property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        //对象名称
        property.Name = "Str22";
        //设置property的类型 
        property.Type = new CodeTypeReference(typeof(System.String));

       //有get
       property.HasGet = true;
       //有set
       property.HasSet = true;
       //添加注释
       property.Comments.Add(new CodeCommentStatement("this is Str"));
       //get
       property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "str")));
       //set
       property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "str"), new CodePropertySetValueReferenceExpression()));
        ///添加到Customerclass类中
        myClass.Members.Add(property);
        */
        //输出文件路径
        string outputFile = Application.dataPath + "/Game/Scripts/StaticData/" + "Static_" + fileName + ".cs";

        //保存
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile, false, new System.Text.UTF8Encoding(false))) // 第二个参数 false 表示覆盖而非追加
        {
            provider.GenerateCodeFromCompileUnit(unit, sw, options);
            Debug.Log(fileName + "已完成");
        }

    }
}
