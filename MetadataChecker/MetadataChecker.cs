using System.Linq;
using uScript.API.Attributes;
using uScript.Core;
using uScript.Module.Main.Classes;

namespace MetadataChecker
{
    [ScriptTypeExtension(typeof(ItemClass))]
    public class MetadataChecker
    {
        [ScriptFunction("get_metadata")]
        public static ExpressionValue metadata([ScriptInstance] ExpressionValue instance)
        {
            if (!(instance.Data is ItemClass item)) return 0;
            var metadata = item.Item.item.metadata.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue
            return ExpressionValue.Array(metadata);
        }
    }
}