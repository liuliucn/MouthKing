using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MouthKing.Core;

[JsonSerializable(typeof(AiRequest))]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(JsonNode))]
public partial class AotJsonSerializerContext : JsonSerializerContext
{

}