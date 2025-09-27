using System.Text.Json;
using System.Text.Json.Serialization;
namespace LVTN_BE_COFFE.Domain.Ultilities
{
    public class JsonHelper
    {
        public static dynamic DeserializeJsonUserHasFunctions(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<dynamic>();
            }
            try
            {
                return JsonSerializer.Deserialize<dynamic>(json) ?? new List<dynamic>();
            }
            catch (JsonException)
            {
                return new List<dynamic>();
            }
        }
    }
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                {
                    throw new JsonException("Date string cannot be null or empty.");
                }
                if (DateOnly.TryParse(dateString, out DateOnly date))
                {
                    return date;
                }
                else
                {
                    throw new JsonException($"Invalid date format: {dateString}. Expected format is 'yyyy-MM-dd'.");
                }
            }

            throw new JsonException("Expected string token for date.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }
}
