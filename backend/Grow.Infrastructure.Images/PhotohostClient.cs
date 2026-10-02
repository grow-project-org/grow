namespace Grow.Infrastructure.Images;

public class PhotohostClient(PhotohostOptions options, IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient client = httpClientFactory.CreateClient();

    public async Task<bool> HealthCheck()
    {
        var url = new Uri(options.Address, "/hc");
        var response = await this.client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var content = await response.Content.ReadAsStringAsync();
        return content == "healthy";
    }
}
