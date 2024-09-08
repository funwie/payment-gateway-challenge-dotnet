using System.Net.Mime;
using System.Text;

using Newtonsoft.Json;

namespace PaymentGateway.Api.Clients;

public class AcquirerClient(HttpClient httpClient) : IAcquirerClient
{
    public async Task<AcquirerResponse> AcquireAsync(AcquirerRequest acquirerRequest,
        CancellationToken cancellationToken)
    {
        string jsonBody =
            JsonConvert.SerializeObject(acquirerRequest, SerializerSettings.DefaultJsonSerializerSettings());
        HttpRequestMessage request = new(HttpMethod.Post, "/payments")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return SerializerSettings.Deserialize<AcquirerResponse>(responseContent);
        }

        string message = $"request to acquire payment failed with status: {response.StatusCode}. ";
        if (response.Content is not null)
        {
            message += await response.Content.ReadAsStringAsync(cancellationToken);
        }

        throw new AcquirerException(message);
    }
}