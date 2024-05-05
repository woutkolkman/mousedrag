using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Menu.Remix;

namespace MouseDrag
{
    public static class Info
    {
        public static void DumpInfo(PhysicalObject obj)
        {
            //string dumpedObject = ObjectDumper.Dump(obj);
            User usr = new User();
            usr.FirstName = "Henk";
            usr.LastName = "De Potvis";
            usr.Address = new Address();
            usr.Address.Street = "A";
            usr.Address.ZipCode = 69;
            usr.Address.City = "B";
            usr.Hobbies = new List<Hobby> {
                new Hobby() { Name = "programming" }
            };
            string dumpedObject = ObjectDumper.Dump(usr);
            Plugin.Logger.LogDebug(dumpedObject);
            UniClipboard.SetText(dumpedObject);
        }


        private class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Address Address { get; set; }
            public IList<Hobby> Hobbies { get; set; }
        }


        private class Hobby
        {
            public string Name { get; set; }
        }


        private class Address
        {
            public string Street { get; set; }
            public int ZipCode { get; set; }
            public string City { get; set; }
        }


        //not my code, but from https://stackoverflow.com/a/10478008
        //more info https://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object/852216#852216
        private class ObjectDumper
        {
            private int _level;
            private readonly int _indentSize;
            private readonly StringBuilder _stringBuilder;
            private readonly List<int> _hashListOfFoundElements;


            private ObjectDumper(int indentSize)
            {
                _indentSize = indentSize;
                _stringBuilder = new StringBuilder();
                _hashListOfFoundElements = new List<int>();
            }


            public static string Dump(object element)
            {
                return Dump(element, 2);
            }


            public static string Dump(object element, int indentSize)
            {
                var instance = new ObjectDumper(indentSize);
                return instance.DumpElement(element);
            }


            private string DumpElement(object element)
            {
                if (element == null || element is ValueType || element is string) {
                    Write(FormatValue(element));
                } else {
                    var objectType = element.GetType();
                    if (!typeof(IEnumerable).IsAssignableFrom(objectType)) {
                        Write("{{{0}}}", objectType.FullName);
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
                                    Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
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
                            object value = fieldInfo != null
                                               ? fieldInfo.GetValue(element)
                                               : propertyInfo.GetValue(element, null);
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
                                    Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
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
                if (args != null)
                    value = string.Format(value, args);
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
