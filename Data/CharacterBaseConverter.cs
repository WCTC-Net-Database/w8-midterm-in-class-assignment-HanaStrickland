using System.Text.Json;
using System.Text.Json.Serialization;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Data;

public class CharacterBaseConverter : JsonConverter<CharacterBase>
{
    private readonly OutputManager _outputManager;

    public CharacterBaseConverter(OutputManager outputManager)
    {
        _outputManager = outputManager;
    }

    public override CharacterBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            var typeProperty = root.GetProperty("$type").GetString();
            var type = typeProperty switch
            {
                "Player" => typeof(Player),
                "Goblin" => typeof(Goblin),
                "Ghost" => typeof(Ghost),
                "Vampire" => typeof(Vampire),
                "Ghoul" => typeof(Ghoul),
                _ => throw new NotSupportedException($"Type {typeProperty} is not supported")
            };
            var character = (CharacterBase)JsonSerializer.Deserialize(root.GetRawText(), type, options);
            character.SetOutputManager(_outputManager);
            return character;
        }
    }

    public override void Write(Utf8JsonWriter writer, CharacterBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}
