using System;
using System.Diagnostics;
using System.Reflection;

namespace CheatSystem
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class CheatClass : Attribute
    {
#if USING_CHEAT_SYSTEM
        public Type ClassType { get; }
#endif

        public CheatClass(Type classType)
        {
#if USING_CHEAT_SYSTEM
            ClassType = classType;
#endif
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class Cheat : Attribute
    {
#if USING_CHEAT_SYSTEM
        public string Name;
#endif
        public Cheat(string CheatName = null)
        {
#if USING_CHEAT_SYSTEM
            if (CheatName != null)
            {
                if (CheatName.Length > 0)
                {
                    Name = CheatName;
                }
            }
#endif
        }
    }
}
