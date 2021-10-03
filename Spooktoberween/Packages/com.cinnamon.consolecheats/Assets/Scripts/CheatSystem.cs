using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace CheatSystem
{
    public class CheatSystem : MonoBehaviour
    {
#if USING_CHEAT_SYSTEM
        static CheatSystem Instance;

        // Contains the method info for every function marked with a cheat attribute defined in a class marked as a cheat class with the assembly attribute
        Dictionary<string, MethodInfo> cheatMethodInfos = new Dictionary<string, MethodInfo>();

        CheatConsole Console = null;

        void Awake()
        {
            if (Instance)
            {
                Debug.LogError("There are multiple cheat system instances");
                Destroy(this);
                return;
            }

            Instance = this;
            // Find all cheat functions and store their method info
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Attribute attribute in assembly.GetCustomAttributes(typeof(CheatClass)))
                {
                    CheatClass cheatClass = (CheatClass) attribute;
                    if (cheatClass != null)
                    {
                        foreach (MethodInfo methodInfo in cheatClass.ClassType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Static))
                        {
                            Cheat cheatAtt = (Cheat)methodInfo.GetCustomAttribute(typeof(Cheat));
                            if (cheatAtt != null)
                            {
                                string cheatName = cheatAtt.Name == null ? methodInfo.Name : cheatAtt.Name;
                                if (cheatMethodInfos.ContainsKey(cheatName))
                                {
                                    MethodInfo firstMemberInfo = cheatMethodInfos[methodInfo.Name];
                                    Debug.AssertFormat(false, "Multiple cheats found with name {0}. First occurrence in type {1}, new occurrence in type {2}", cheatName, firstMemberInfo.DeclaringType, methodInfo.DeclaringType);

                                    continue;
                                }

                                cheatMethodInfos.Add(cheatName, methodInfo);
                            }
                        }
                    }
                }
            }

            Console = new CheatConsole();
        }

        void OnGUI()
        {
            Console.DrawConsole();
        }

        public static void ExecuteCheat(string CheatString)
        {
            if (!Instance || CheatString == null || CheatString.Length == 0) return;

            List<string> Tokens = new List<string>();
            SplitCheatString(CheatString, ref Tokens);

            if (Instance.cheatMethodInfos.TryGetValue(Tokens[0], out MethodInfo cheatMethodInfo))
            {
                ParameterInfo[] paramInfos = cheatMethodInfo.GetParameters();

                object[] methodParams = new object[paramInfos.Length];

                foreach (ParameterInfo paramInfo in cheatMethodInfo.GetParameters())
                {
                    if (paramInfo.Position + 1 >= Tokens.Count)
                    {
                        if (!paramInfo.IsOptional)
                        {
                            Debug.LogErrorFormat("cheat {0}: parameter {1}: No value given for non-optional parameter", Tokens[0], paramInfo.Name);
                            return;
                        }

                        methodParams[paramInfo.Position] = paramInfo.DefaultValue;
                    }
                    else if (paramInfo.ParameterType == typeof(string))
                    {
                        methodParams[paramInfo.Position] = Tokens[paramInfo.Position + 1];
                    }
                    else
                    {
                        methodParams[paramInfo.Position] = paramInfo.ParameterType.InvokeMember("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                            null, null, new object[] {Tokens[paramInfo.Position + 1]});
                    }
                }

                if (cheatMethodInfo.IsStatic)
                {
                    cheatMethodInfo.Invoke(null, methodParams);
                }
                else
                {
                    foreach (object instance in FindObjectsOfType(cheatMethodInfo.DeclaringType))
                    {
                        cheatMethodInfo.Invoke(instance, methodParams);
                    }
                }
            }
        }

        public static void SplitCheatString(string CheatString, ref List<string> OutTokens)
        {
            OutTokens.Clear();

            bool bIsSplitChar = true;
            char QuoteChar = '\0';
            string currentString = "";
            for (int i = 0; i < CheatString.Length; ++i)
            {
                if (CheatString[i] == '\0')
                {
                    OutTokens.Add(currentString);
                    currentString = "";
                    break;
                }

                if (CheatString[i] == ' ' && QuoteChar == '\0')
                {
                    if (!bIsSplitChar)
                    {
                        bIsSplitChar = true;
                        OutTokens.Add(currentString);
                        currentString = "";
                    }
                    continue;
                }

                if (CheatString[i] == '\'' || CheatString[i] == '\"')
                {
                    if (QuoteChar == CheatString[i] && (i+1 == CheatString.Length || CheatString[i+1] == ' ' || CheatString[i+1] == '\0'))
                    {
                        QuoteChar = '\0';
                        bIsSplitChar = true;
                        OutTokens.Add(currentString);
                        currentString = "";
                        continue;
                    }
                    else if (QuoteChar == '\0' && bIsSplitChar)
                    {
                        QuoteChar = CheatString[i];
                        continue;
                    }
                }

                bIsSplitChar = false;

                currentString += CheatString[i];
            }

            if (currentString.Length > 0)
            {
                OutTokens.Add(currentString);
            }
        }

        public static void GetCheatSuggestions(string ReferenceText, int maxSuggestions, ref List<string> Suggestions)
        {
            Suggestions.Clear();

            if (!Instance || maxSuggestions <= 0 || ReferenceText == null || ReferenceText.Length == 0) return;

            ReferenceText = ReferenceText.ToLower();

            string[] Tokens = ReferenceText.Split();

            if(Tokens.Length == 0 || Tokens[0].Length == 0) return;

            foreach (string CheatName in Instance.cheatMethodInfos.Keys)
            {
                if(Tokens[0].Length > CheatName.Length || (Tokens.Length > 1 && Tokens[0].Length != CheatName.Length)) continue;

                string lowerCheatName = CheatName.ToLower();

                bool bMatch = true;
                for (int i = 0; i < Tokens[0].Length; ++i)
                {
                    if(Tokens[0][i] == lowerCheatName[i]) continue;

                    bMatch = false;
                    break;
                }

                if (bMatch)
                {
                    Suggestions.Add(GetCheatUsage(CheatName));
                    if (Suggestions.Count == maxSuggestions) break;
                }
            }
        }

        // Returns a string to display the usage of the given cheat
        public static string GetCheatUsage(string cheatName)
        {
            string usage = cheatName;

            if (Instance)
            {
                if (Instance.cheatMethodInfos.TryGetValue(cheatName, out MethodInfo cheatInfo))
                {
                    if (cheatInfo != null)
                    {
                        foreach (ParameterInfo parameterInfo in cheatInfo.GetParameters())
                        {
                            usage += " ";
                            usage += parameterInfo.ParameterType;
                            usage += ":";
                            usage += parameterInfo.Name;

                            if (parameterInfo.IsOptional)
                            {
                                usage += "[" + parameterInfo.DefaultValue + "]";
                            }
                        }
                    }
                }
            }

            return usage;
        }
#endif // USING_CHEAT_SYSTEM
    }
}
