using JsonParser.JsonStructures;

namespace AiLinLib.Helpers
{
    public static class JsonHelper
    {
        public static void SetValue(this JsonPairs pairs, string key, string value)
        {
            pairs.KeyValues[key] = new JsonValue<string> { Value = value };
        }
    }
}
