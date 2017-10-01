using System;

namespace AiLinWpf.Helpers
{
    public static class InterpretationHelper
    {
		public static bool IsNontrivial(object val)
        {
            if (val is string s)
            {
                return !string.IsNullOrWhiteSpace(s);
            }
            else if (val.GetType().IsValueType)
            {
                var defaultVal = Activator.CreateInstance(val.GetType());
                return val != defaultVal;
            }
            else
            {
                return val != null;
            }
        }
    }
}
