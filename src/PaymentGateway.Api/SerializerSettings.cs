using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace PaymentGateway.Api;

public class SerializerSettings
{
    public static JsonSerializerSettings DefaultJsonSerializerSettings()
    {
        JsonSerializerSettings setting = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            Formatting = Formatting.Indented
        };
        setting.Converters.Add(new StringEnumConverter());

        return setting;
    }

    public static T? Deserialize<T>(string responseContent)
    {
        return JsonConvert.DeserializeObject<T>(responseContent, DefaultJsonSerializerSettings());
    }
}