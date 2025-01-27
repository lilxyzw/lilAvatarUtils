using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("jp.lilxyzw.avatarutils")]

namespace jp.lilxyzw.avatarutils
{
    internal static class DocsGenerator
    {
        private static readonly BindingFlags DEFAULT_FLAG = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static void Generate(Func<Type,string> funcPath, Func<string,string> localize, Func<FieldInfo,(string,string)> funcHeader, Func<FieldInfo,string> funcTooltip, Func<FieldInfo,bool> funcNeedToDraw, Action<Type, StringBuilder> actionPerType, params Type[] types)
        {
            foreach(var type in types)
            {
                // [Docs]がない場合はドキュメントを生成しない
                var docs = type.GetCustomAttribute<DocsAttribute>();
                if(docs == null) continue;

                var sb = new StringBuilder();
                // ヘッダー
                sb.Header(1, docs.title, localize);

                // メニュー位置
                var menuLocation = type.GetCustomAttribute<DocsMenuLocationAttribute>()?.location;
                if(!string.IsNullOrEmpty(menuLocation)) sb.Text(localize("Menu Location") + $" => `{menuLocation}`", localize);

                // 説明
                sb.Text(docs.texts, localize);

                actionPerType(type, sb);

                // 使い方
                var howto = type.GetCustomAttribute<DocsHowToAttribute>()?.texts;
                if(howto != null && howto.Length > 0)
                {
                    sb.Header(2, "How to use", localize);
                    sb.Text(howto, localize);
                }

                // 各プロパティの表
                var fields = type.GetFields(DEFAULT_FLAG);
                if(fields.Length != 0)
                {
                    bool isFirst = true;
                    bool isFirstDocsField = true;
                    void AddProperty(string name, string tooltip)
                    {
                        isFirst = false;
                        if(isFirstDocsField)
                        {
                            sb.Header(2, "Property", localize);
                            sb.TableHeader(localize);
                            isFirstDocsField = false;
                        }
                        sb.AppendLine($"|{localize(name)}|{localize(tooltip)}|");
                    }

                    foreach(var field in fields)
                    {
                        var header = funcHeader(field);

                        if(!string.IsNullOrEmpty(header.Item1))
                        {
                            // ヘッダーの場合はヘッダーと表の先頭を作成
                            if(!isFirst) sb.AppendLine();
                            sb.Header(2, header.Item1, localize);
                            if(!string.IsNullOrEmpty(header.Item2)) sb.Text(header.Item2, localize);
                            sb.TableHeader(localize);
                            isFirst = false;
                            isFirstDocsField = false;
                        }

                        if(field.IsStatic && field.FieldType == typeof(string[]) && field.GetCustomAttribute<DocsFieldAttribute>() != null)
                        {
                            // [DocsField]かつstring[]型の場合は値から取得
                            if(field.GetValue(null) is not string[] value || value.Length < 2)
                            {
                                throw new Exception($"{type.FullName}.{field.Name} has DocsFieldAttribute, but string array is null or shorter than 2.");
                            }
                            AddProperty(value[0], value[1]);
                        }
                        else if(field.GetCustomAttribute<DocsGetStringsAttribute>()?.func is Func<string[][]> getstrings)
                        {
                            // [DocsGetStrings]の場合はそこから取得
                            var strings = getstrings();
                            foreach(var ss in strings)
                                AddProperty(ss[0], ss[1]);
                        }
                        else if(funcNeedToDraw(field))
                        {
                            // その他はユーザー定義の方法で取得
                            var tooltip = funcTooltip(field);
                            if(!string.IsNullOrEmpty(tooltip))
                                AddProperty(field.Name.ToDisplayName(), tooltip);
                        }
                    }

                    if(!isFirst) sb.AppendLine();
                }

                var path = funcPath(type);
                var directory = Path.GetDirectoryName(path);
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
        }

        private static StringBuilder Header(this StringBuilder sb, int sharps, string text, Func<string,string> func) => sb.AppendLine(new string('#', sharps) + ' ' + func(text)).AppendLine();
        private static StringBuilder Text(this StringBuilder sb, string text, Func<string,string> func) => sb.AppendLine(func(text)).AppendLine();
        private static StringBuilder Text(this StringBuilder sb, string[] texts, Func<string,string> func)
        {
            if(texts == null || texts.Length == 0) return sb;
            foreach(var t in texts) sb.AppendLine(func(t));
            return sb.AppendLine();
        }

        private static StringBuilder TableHeader(this StringBuilder sb, Func<string,string> func)
            => sb.AppendLine($"|{func("Name")}|{func("Description")}|").AppendLine($"|-|-|");

        private static string ToDisplayName(this string name)
        {
            name = string.Concat(name.Select(c => char.IsUpper(c) ? " "+c : c.ToString())).Trim();
            return char.ToUpper(name[0]) + name[1..];
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class DocsAttribute : Attribute
    {
        public string title;
        public string[] texts;
        public DocsAttribute(string title, params string[] texts)
        {
            this.title = title;
            this.texts = texts;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class DocsFieldAttribute : Attribute {}

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class DocsGetStringsAttribute : Attribute
    {
        public Func<string[][]> func;
        public DocsGetStringsAttribute(Type type, string method)
            => func = () => type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null,null) as string[][];
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class DocsMenuLocationAttribute : Attribute
    {
        public string location;
        public DocsMenuLocationAttribute(string location) => this.location = location;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class DocsHowToAttribute : Attribute
    {
        public string[] texts;
        public DocsHowToAttribute(params string[] texts)
        {
            this.texts = texts;
        }
    }
}
