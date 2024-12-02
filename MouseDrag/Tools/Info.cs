using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MouseDrag
{
    public static class Info
    {
        public static string DumpInfo(object obj, int? lvl = null)
        {
            int maxLevel = Options.infoDepth?.Value ?? 3;
            if (lvl.HasValue)
                maxLevel = lvl.Value;
            string dumpedObject = ObjectDumper.Dump(obj, 2, maxLevel);
            //Plugin.Logger.LogDebug(dumpedObject);
            Menu.Remix.UniClipboard.SetText(dumpedObject);
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Info.DumpInfo, copied to clipboard");
            return dumpedObject;
        }


        //not my code, but adapted from https://stackoverflow.com/a/10478008
        //more info https://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object/852216#852216
        private class ObjectDumper
        {
            private int _level;
            private int _maxLevel; //added maxLevel
            private readonly int _indentSize;
            private readonly StringBuilder _stringBuilder;
            private readonly List<int> _hashListOfFoundElements;


            private ObjectDumper(int indentSize, int maxLevel) //added maxLevel
            {
                _indentSize = indentSize;
                _stringBuilder = new StringBuilder();
                _hashListOfFoundElements = new List<int>();
                _maxLevel = maxLevel; //added maxLevel
            }


            public static string Dump(object element, int indentSize, int maxLevel) //added maxLevel
            {
                var instance = new ObjectDumper(indentSize, maxLevel);
                return instance.DumpElement(element);
            }


            private string DumpElement(object element)
            {
                if (element == null || element is ValueType || element is string) {
                    Write(FormatValue(element));
                } else {
                    var objectType = element.GetType();
                    if (!typeof(IEnumerable).IsAssignableFrom(objectType)) {
                        if (_level >= _maxLevel) { //added early return if max level is reached
                            Write("{{{0}}} <-- max level reached", objectType.FullName);
                            return _stringBuilder.ToString();
                        } else {
                            Write("{{{0}}}", objectType.FullName);
                        }
                        _hashListOfFoundElements.Add(element.GetHashCode());
                        _level++;
                    }
                    var enumerableElement = element as IEnumerable;
                    if (enumerableElement != null) {
                        foreach (object item in enumerableElement) {
                            if (item is IEnumerable && !(item is string)) {
                                _level++;
                                DumpElement(item);
                                _level--;
                            } else {
                                if (!AlreadyTouched(item)) {
                                    DumpElement(item);
                                } else {
                                    Write("{{{0}}} <-- bidirectional reference or already touched", item.GetType().FullName);
                                }
                            }
                        }
                    } else {
                        MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var memberInfo in members) {
                            var fieldInfo = memberInfo as FieldInfo;
                            var propertyInfo = memberInfo as PropertyInfo;
                            if (fieldInfo == null && propertyInfo == null)
                                continue;
                            var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                            object value = null;
                            try {
                                value = fieldInfo != null ? fieldInfo.GetValue(element) : propertyInfo.GetValue(element, null);
                            } catch (Exception ex) { //added exception handler
                                Plugin.Logger.LogWarning("Info.ObjectDumper.DumpElement exception while getting field or property info for a member: " + ex?.ToString());
                            }
                            if (type.IsValueType || type == typeof(string)) {
                                Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                            } else {
                                var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                                Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                                var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                                _level++;
                                if (!alreadyTouched) {
                                    DumpElement(value);
                                } else {
                                    Write("{{{0}}} <-- bidirectional reference or already touched", value.GetType().FullName);
                                }
                                _level--;
                            }
                        }
                    }
                    if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                        _level--;
                }
                return _stringBuilder.ToString();
            }


            private bool AlreadyTouched(object value)
            {
                if (value == null)
                    return false;
                var hash = value.GetHashCode();
                for (var i = 0; i < _hashListOfFoundElements.Count; i++) {
                    if (_hashListOfFoundElements[i] == hash)
                        return true;
                }
                return false;
            }


            private void Write(string value, params object[] args)
            {
                var space = new string(' ', _level * _indentSize);
                if (args != null) {
                    try {
                        value = string.Format(value, args);
                    } catch (FormatException) { //added to catch format exceptions
                        Plugin.Logger.LogWarning("Info.ObjectDumper.Write, caught FormatException while formatting: " + value);
                    }
                }
                _stringBuilder.AppendLine(space + value);
            }


            private string FormatValue(object o)
            {
                if (o == null)
                    return ("null");
                if (o is DateTime)
                    return (((DateTime)o).ToShortDateString());
                if (o is string)
                    return string.Format("\"{0}\"", o);
                if (o is char && (char)o == '\0')
                    return string.Empty;
                if (o is ValueType)
                    return (o.ToString());
                if (o is IEnumerable)
                    return ("...");
                return ("{ }");
            }
        }
    }
}
