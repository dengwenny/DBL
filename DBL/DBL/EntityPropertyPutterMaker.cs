using System;
using System.Text;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using DBL.Mapping;
using System.Data;
using System.Reflection;
using System.Data.Common;


namespace DBL
{
    class EntityPropertyPutterMaker
    {
        private bool m_includeDebugInfo;
        private string providerName;
        public string ProviderName
        {
            get { return providerName; }
            set { providerName = value; }
        }
        public IEntityPropertyPutter<T> Make<T>() where T : class
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(typeof(T).Name);
            declaration.BaseTypes.Add(typeof(IEntityPropertyPutter<T>));
            declaration = MakeEntityTypeNameProperty<T>(declaration);
            CodeNamespace namespace2 = new CodeNamespace(declaration.Name);
            namespace2.Imports.Add(new CodeNamespaceImport("System"));
            namespace2.Imports.Add(new CodeNamespaceImport(providerName));
            //namespace2.Imports.Add(new CodeNamespaceImport("MySql.Data.MySqlClient"));
            //namespace2.Imports.Add(new CodeNamespaceImport("System.Data.OracleClient"));
            //namespace2.Imports.Add(new CodeNamespaceImport("System.Data.SQLite"));
            CodeMemberMethod insert = new CodeMemberMethod();
            insert = this.make_insert_method<T>(insert);
            CodeMemberMethod update = this.make_update_method<T>();
            CodeMemberMethod datarow = this.Make_Datarow_Method<T>();
            CodeMemberMethod reader = this.Make_DataReader_Method<T>();
            CodeMemberMethod datarowcheck = this.Make_Datarow_Method_Check<T>();
            CodeMemberMethod readercheck = this.Make_DataReader_Method_Check<T>();
            
            declaration.Members.Add(insert);
            declaration.Members.Add(update);
            declaration.Members.Add(datarow);
            declaration.Members.Add(reader);
            declaration.Members.Add(datarowcheck);
            declaration.Members.Add(readercheck);
            namespace2.Types.Add(declaration);
            unit.Namespaces.Add(namespace2);
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters options = new CompilerParameters();
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Data.dll");
            options.ReferencedAssemblies.Add(base.GetType().Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(T).Assembly.Location);
            options.GenerateInMemory = true;
            CompilerResults results = provider.CompileAssemblyFromDom(options, new CodeCompileUnit[] { unit });
            if (results.NativeCompilerReturnValue == 0)
            {
                return (IEntityPropertyPutter<T>)results.CompiledAssembly.CreateInstance(namespace2.Name + "." + declaration.Name);
            }
            string str2 = "";
            if (results.Errors.Count > 0)
            {
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    str2 = str2 + results.Errors[i].ErrorText;
                }
            }
            throw new Exception("编译失败 (" + str2 + ")");
        }



        /// <summary>
        /// 生成字段属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetClass"></param>
        /// <returns></returns>
        public CodeTypeDeclaration MakeEntityTypeNameProperty<T>(CodeTypeDeclaration targetClass) where T : class
        {
            CodeMemberField field = null;
            field = new CodeMemberField
            {
                Attributes = MemberAttributes.Private
            };
            field = new CodeMemberField(typeof(string), "m_currPropName");
            targetClass.Members.Add(field);
            CodeMemberProperty property = null;
            property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(typeof(string)),
                Name = "CurrentPropName"
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_currPropName")));
            targetClass.Members.Add(property);
            property = new CodeMemberProperty()
            {
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(typeof(string)),
                Name = "EntityTypeName"
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(typeof(T).Name)));
            targetClass.Members.Add(property);
            field = new CodeMemberField(typeof(string), "m_currDBColName");
            targetClass.Members.Add(field);
            property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(typeof(string)),
                Name = "CurrentDBColName"
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_currDBColName")));
            targetClass.Members.Add(property);
            return targetClass;
        }


        /// <summary>
        /// datarow转换实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CodeMemberMethod Make_Datarow_Method<T>() where T : class {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            method.ReturnType = new CodeTypeReference(typeof(void));
            method.Name = "PutEntityProperties";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "entity"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataRow), "datarow"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataColumnCollection),"datacolumn"));
            Type type = typeof(T);
            object[] customAttributes = type.GetCustomAttributes(typeof(DataTableAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            PropertyInfo[] properties = type.GetProperties();
            if ((properties == null) || (properties.Length <= 0))
            {
                return null;
            }
            if (properties == null && properties.Length <= 0) {
                return method;
            }
            foreach (PropertyInfo info in properties)
            {
                object[] attributes = info.GetCustomAttributes(typeof(DataColumnAttribute), false);
                if ((attributes != null) && (attributes.Length > 0))
                {
                    DataColumnAttribute attr = attributes[0] as DataColumnAttribute;
                    this.MakeMethodContent(method, info, attr, this.IncludeDebugInformation,true);
                }
            }

            return method;
        }

        private CodeMemberMethod Make_DataReader_Method<T>() where T : class
        {
            CodeMemberMethod targetMethod = null;
            targetMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(typeof(void)),
                Name = "PutEntityProperties"
            };
            targetMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "entity"));
            targetMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DbDataReader), "dr"));
            targetMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataColumnCollection), "datacolumn"));
            Type type = typeof(T);
            object[] customAttributes = type.GetCustomAttributes(typeof(DataTableAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            PropertyInfo[] properties = type.GetProperties();
            if ((properties == null) || (properties.Length <= 0))
            {
                return null;
            }
            foreach (PropertyInfo info in properties)
            {
                object[] objArray2 = info.GetCustomAttributes(typeof(DataColumnAttribute), false);
                if ((objArray2 != null) && (objArray2.Length > 0))
                {
                    DataColumnAttribute attr = objArray2[0] as DataColumnAttribute;
                    targetMethod = this.MakeMethodContent(targetMethod,info,attr,this.IncludeDebugInformation,true);
                }
            }
            return targetMethod;
        }

        /// <summary>
        /// datarow转换实体,实体属性必须和datarow列一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CodeMemberMethod Make_Datarow_Method_Check<T>() where T : class
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            method.ReturnType = new CodeTypeReference(typeof(void));
            method.Name = "PutEntityProperties";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "entity"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataRow), "datarow"));
            Type type = typeof(T);
            object[] customAttributes = type.GetCustomAttributes(typeof(DataTableAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            PropertyInfo[] properties = type.GetProperties();
            if ((properties == null) || (properties.Length <= 0))
            {
                return null;
            }
            if (properties == null && properties.Length <= 0)
            {
                return method;
            }
            foreach (PropertyInfo info in properties)
            {
                object[] attributes = info.GetCustomAttributes(typeof(DataColumnAttribute), false);
                if ((attributes != null) && (attributes.Length > 0))
                {
                    DataColumnAttribute attr = attributes[0] as DataColumnAttribute;
                    this.MakeMethodContent(method, info, attr, this.IncludeDebugInformation, false);
                }
            }

            return method;
        }

        /// <summary>
        /// DataReader转换实体,实体属性必须和DataReader列一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private CodeMemberMethod Make_DataReader_Method_Check<T>() where T : class
        {
            CodeMemberMethod targetMethod = null;
            targetMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(typeof(void)),
                Name = "PutEntityProperties"
            };
            targetMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "entity"));
            targetMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DbDataReader), "dr"));
            Type type = typeof(T);
            object[] customAttributes = type.GetCustomAttributes(typeof(DataTableAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            PropertyInfo[] properties = type.GetProperties();
            if ((properties == null) || (properties.Length <= 0))
            {
                return null;
            }
            foreach (PropertyInfo info in properties)
            {
                object[] objArray2 = info.GetCustomAttributes(typeof(DataColumnAttribute), false);
                if ((objArray2 != null) && (objArray2.Length > 0))
                {
                    DataColumnAttribute attr = objArray2[0] as DataColumnAttribute;
                    targetMethod = this.MakeMethodContent(targetMethod, info, attr, this.IncludeDebugInformation, false);
                }
            }
            return targetMethod;
        }

        /// <summary>
        /// 给实体赋值
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="info">实体每个属性</param>
        /// <param name="attr">实体每个特性</param>
        /// <param name="IncludeDebugInformation">调试属性</param>
        /// <param name="ischeckcolomn">是否判断数据列和实体属性一致</param>
        /// <returns></returns>
        public CodeMemberMethod MakeMethodContent(CodeMemberMethod targetMethod,PropertyInfo info,DataColumnAttribute attr, bool IncludeDebugInformation,bool ischeckcolomn){
            string name = targetMethod.Parameters[0].Name;
            string str2 = targetMethod.Parameters[1].Name;
            
            string variableName = string.Format("{0}.{1}", name, info.Name);
            string str4 = string.Format("{0}[\"{1}\"]", str2, attr.Name);
            CodeVariableReferenceExpression left = new CodeVariableReferenceExpression(variableName);
            CodeVariableReferenceExpression expression = new CodeVariableReferenceExpression(str4);
            if (this.IncludeDebugInformation)
            {
                targetMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("this.m_currPropName"), new CodePrimitiveExpression(info.Name)));
                targetMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("this.m_currDBColName"), new CodePrimitiveExpression(attr.Name)));
            }

            if (ischeckcolomn)
            {
                string colmn = targetMethod.Parameters[2].Name;
                //表达式datacolumn.Contains("tt")判断是否存在列
                CodeMethodInvokeExpression addparameter = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression(colmn), "Contains", new CodePrimitiveExpression(attr.Name));
                //判断
                CodeConditionStatement ifcolmnstatement = new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression(addparameter, CodeBinaryOperatorType.BooleanAnd, new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str4), CodeBinaryOperatorType.IdentityInequality, new CodeVariableReferenceExpression("System.DBNull.Value")))
                };
                ifcolmnstatement.TrueStatements.Add(new CodeAssignStatement(left, new CodeCastExpression(info.PropertyType, expression)));
                if ("System.String".Equals(info.PropertyType.FullName))
                {
                    ifcolmnstatement.FalseStatements.Add(new CodeAssignStatement(left, new CodeVariableReferenceExpression("string.Empty")));
                }
                targetMethod.Statements.Add(ifcolmnstatement);
            }
            else
            {
                //列值是否为空--
                CodeConditionStatement statement = new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(str4), CodeBinaryOperatorType.IdentityInequality, new CodeVariableReferenceExpression("System.DBNull.Value"))
                };
                statement.TrueStatements.Add(new CodeAssignStatement(left, new CodeCastExpression(info.PropertyType, expression)));
                if ("System.String".Equals(info.PropertyType.FullName))
                {
                    statement.FalseStatements.Add(new CodeAssignStatement(left, new CodeVariableReferenceExpression("string.Empty")));
                }
                //statement.FalseStatements.Add(new CodeAssignStatement(left, new CodeVariableReferenceExpression("default(" + info.PropertyType + ")")));
                targetMethod.Statements.Add(statement);
            }
            
            return targetMethod;
        }

        // 生成修改方法
        public CodeMemberMethod make_update_method<T>() where T : class {
            CodeMemberMethod method = new CodeMemberMethod() {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "updateEntity",
                ReturnType = new CodeTypeReference(typeof(int))
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T),"entity"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDBHelper),"ih"));
            //CodeMethodInvokeExpression methodinvoke = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("Factory"), "Execute", new CodeExpression[] { });
           // CodeVariableReferenceExpression left = new CodeVariableReferenceExpression("IDBHelper ih");
            //method.Statements.Add(new CodeAssignStatement(left, methodinvoke));//IDBHelper ih = Factory.Execute();
            Type type = typeof(T);
            string tablename = "";
            object[] tableAttributes = type.GetCustomAttributes(typeof(DataTableAttribute),false);
            if ((tableAttributes == null) || (tableAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            DataTableAttribute tableAttribute = tableAttributes[0] as DataTableAttribute;
            tablename = tableAttribute.Name;
            List<string> entitynames = new List<string>();//实体属性名称
            List<string> attributenames = new List<string>();//映射字段名称
            List<string> datatypes = new List<string>();//字段类型
            List<bool> isidentity = new List<bool>();//是否主键
            foreach (var info in type.GetProperties())
            {
                object[] cuattribute = info.GetCustomAttributes(typeof(DataColumnAttribute), false);//获取该属性字段映射
                object[] cuidentityattribute = info.GetCustomAttributes(typeof(DataColumnIdentityAttribute), false);//获取该字段主键字段映射
                if(cuidentityattribute!=null && cuidentityattribute.Length>0){
                    DataColumnIdentityAttribute idenattri = cuidentityattribute[0] as DataColumnIdentityAttribute;
                    if(idenattri.IsIdentity){
                        isidentity.Add(true);
                    }else{
                        isidentity.Add(false);
                    }
                }else{
                    isidentity.Add(false);
                }
                if(cuattribute!=null && cuattribute.Length>0){
                    DataColumnAttribute columnattribute = cuattribute[0] as DataColumnAttribute;
                    attributenames.Add(columnattribute.Name);
                    entitynames.Add(info.Name);
                    string ptype = info.PropertyType.Name;//获得每个属性的类型
                    if (!(ptype == "Int32"))
                    {
                        if (ptype == "String")
                        {
                            datatypes.Add("System.Data.DbType.String");
                        }
                        if (ptype == "DateTime")
                        {
                            datatypes.Add("System.Data.DbType.DateTime");
                        }
                        if (ptype == "Decimal")
                        {
                            datatypes.Add("System.Data.DbType.Decimal");
                        }
                        if (ptype == "Int64")
                        {
                            datatypes.Add("System.Data.DbType.Int64");
                        }
                    }
                    else
                    {
                        datatypes.Add("System.Data.DbType.Int32");
                    }
                }
            }
            string updateSetStr = "";//构建修改的set字段
            string updateWhereStr = "";//构建修改的where条件
            for (int i = 0; i < attributenames.Count; ++i)
            {
                if(isidentity[i]){
                    updateWhereStr += attributenames[i]+"=@"+attributenames[i];
                }else{
                    updateSetStr += attributenames[i]+"=@"+attributenames[i]+",";
                }
            }
            string updatesql = "UPDATE " + tablename + "  SET " + updateSetStr.TrimEnd(new char[] { ',' }) + " WHERE " + updateWhereStr;
            CodeVariableReferenceExpression vdstring = new CodeVariableReferenceExpression("string updatesql");
            CodeVariableReferenceExpression vdupdatesql = new CodeVariableReferenceExpression("\""+updatesql+"\"");
            CodeAssignStatement casupdatesql = new CodeAssignStatement(vdstring, vdupdatesql);
            method.Statements.Add(casupdatesql);
            //SqlParameter[] sss = new SqlParameter[10]; 
            
            for (int i = 0; i < attributenames.Count; i++)
            {
                CodeMethodInvokeExpression addparameter = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("ih"), "setDBparameter", new CodeExpression[] { new CodePrimitiveExpression("@" + attributenames[i]), new CodeVariableReferenceExpression(datatypes[i]), new CodeVariableReferenceExpression("entity." + entitynames[i]) });
                method.Statements.Add(addparameter);
            }
            CodeVariableReferenceExpression exobj = new CodeVariableReferenceExpression("int obj");
            CodeMethodInvokeExpression ExecuteScalar = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("ih"), "ExecuteNonQuery", new CodeExpression[] { new CodeVariableReferenceExpression("updatesql"), new CodeVariableReferenceExpression("System.Data.CommandType.Text") });//ih.ExecuteNonQuery();
            method.Statements.Add(new CodeAssignStatement(exobj, ExecuteScalar));
            CodeArgumentReferenceExpression expression = new CodeArgumentReferenceExpression("obj");
            CodeMethodReturnStatement statement5 = new CodeMethodReturnStatement(expression);
            method.Statements.Add(statement5);
            return method;
        }
        //生成新增方法
        public CodeMemberMethod make_insert_method<T>(CodeMemberMethod method) where T:class {

            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            method.Name = "addEntity";
            method.ReturnType = new CodeTypeReference(typeof(int));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T),"entity"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDBHelper),"ih"));
            //CodeMethodInvokeExpression methodinvoke = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("Factory"), "Execute", new CodeExpression[] { });
            //CodeVariableReferenceExpression left = new CodeVariableReferenceExpression("IDBHelper ih");
            //method.Statements.Add(new CodeAssignStatement(left, methodinvoke));//IDBHelper ih = Factory.Execute();

            Type type = typeof(T);//获取传入T的类型
            object [] customAttributes = type.GetCustomAttributes(typeof(DataTableAttribute), false);//类映射的表名
            if ((customAttributes == null) || (customAttributes.Length <= 0))
            {
                throw new MappingException(string.Format("类 {0} 未标记 DataTable 属性 ( Unlabeled [DataTable] Attribute On Class {0} )", type.Name));
            }
            string tablename = "";//表名
            DataTableAttribute attribute = customAttributes[0] as DataTableAttribute;
            tablename = attribute.Name;
            List<string> dcaValues = new List<string>();//映射表的字段集合
            List<string> entityPepers = new List<string>();// 实体属性集合
            List<string> dbTypes = new List<string>();//每个属性的数据库类型
            foreach (var item in type.GetProperties())
            {
                object[] column = item.GetCustomAttributes(typeof(DataColumnAttribute), false);//类映射的表名
                object[] columnIdentity = item.GetCustomAttributes(typeof(DataColumnIdentityAttribute), false);//类映射的表名
                if (columnIdentity != null && columnIdentity.Length > 0) { }
                else if (column != null && column.Length > 0)
                {
                    DataColumnAttribute attribute3 = column[0] as DataColumnAttribute;
                    dcaValues.Add(attribute3.Name);
                    entityPepers.Add(item.Name);
                    string ptype = item.PropertyType.Name;
                    if (!(ptype == "Int32"))
                    {
                        if (ptype == "String")
                        {
                            dbTypes.Add("System.Data.DbType.String");
                        }
                        if (ptype == "DateTime")
                        {
                            dbTypes.Add("System.Data.DbType.DateTime");
                        }
                        if (ptype == "Decimal")
                        {
                            dbTypes.Add("System.Data.DbType.Decimal");
                        }
                        if (ptype == "Int64")
                        {
                            dbTypes.Add("System.Data.DbType.Int64");
                        }
                    }
                    else
                    {
                        dbTypes.Add("System.Data.DbType.Int32");
                    }
                }
            }
            string sql = "INSERT INTO " + tablename + " (" + string.Join(",", dcaValues) + ")VALUES(" + string.Join(",", (IEnumerable<string>)(from DC in dcaValues select DC = "@" + DC)) + ");select @@IDENTITY";
            CodeVariableReferenceExpression insertsql = new CodeVariableReferenceExpression("string insertsql");
            CodeVariableReferenceExpression sqlvalue = new CodeVariableReferenceExpression("\""+sql+"\"");
            method.Statements.Add(new CodeAssignStatement(insertsql, sqlvalue));
            //初始化SqlParameter
            //CodeSnippetStatement css = new CodeSnippetStatement("SqlParameter[] parameters ={};");
            //method.Statements.Add(css);
            
            //构建sql参数
            CodeVariableReferenceExpression entityvalue = new CodeVariableReferenceExpression("entity");
            for (int i = 0; i < dcaValues.Count; i++)
            {
                CodeMethodInvokeExpression addparameter = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("ih"), "setDBparameter", new CodeExpression[] { new CodePrimitiveExpression("@" + dcaValues[i]), new CodeVariableReferenceExpression(dbTypes[i]), new CodePropertyReferenceExpression(entityvalue, entityPepers[i]) });
                method.Statements.Add(addparameter);
            }
            CodeVariableReferenceExpression exobj = new CodeVariableReferenceExpression("object obj");
            CodeMethodInvokeExpression ExecuteScalar = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("ih"), "ExecuteScalar", new CodeExpression[] { new CodeVariableReferenceExpression("insertsql"), new CodeVariableReferenceExpression("System.Data.CommandType.Text") });//ih.ExecuteScalar();
            method.Statements.Add(new CodeAssignStatement(exobj, ExecuteScalar));
            CodeVariableReferenceExpression returnint = new CodeVariableReferenceExpression("int rInt");
            CodeMethodInvokeExpression returenintc = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.Convert"), "ToInt32", new CodeExpression[] { new CodeArgumentReferenceExpression("obj") });
            CodeAssignStatement statement4 = new CodeAssignStatement(returnint, returenintc);
            method.Statements.Add(statement4);
            CodeArgumentReferenceExpression expression = new CodeArgumentReferenceExpression("rInt");
            CodeMethodReturnStatement statement5 = new CodeMethodReturnStatement(expression);
            method.Statements.Add(statement5);


            //SqlParameter[] parameters ={};
            //parameters[0] = new SqlParameter("", System.Data.SqlDbType.Date);
            return method;
        }


        /// <summary>
        /// 输出类
        /// </summary>
        /// <param name="nspace"></param>
        /// <returns></returns>
        private string GengerCode(CodeNamespace nspace)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            CodeGeneratorOptions geneOptions = new CodeGeneratorOptions();//代码生成选项
            geneOptions.BlankLinesBetweenMembers = false;

            geneOptions.BracingStyle = "C";

            geneOptions.ElseOnClosing = true;

            geneOptions.IndentString = "    ";
            CodeDomProvider.GetCompilerInfo("C#").CreateProvider().GenerateCodeFromNamespace(nspace, sw, geneOptions);//代码生成
            sw.Close();
            return sb.ToString();

        }
        // Properties
        public bool IncludeDebugInformation
        {
            get
            {
                return this.m_includeDebugInfo;
            }
            set
            {
                this.m_includeDebugInfo = value;
            }
        }

    }
}
