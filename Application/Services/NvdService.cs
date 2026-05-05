namespace Application.Services;

using Application.Interfaces;
using System.Net.Http.Json;
using Shared.Dtos;

public class NvdService(HttpClient httpClient) : INvdService
{
    // TODO Initialize the database with the NVD data
    public async Task InitializeDatabase() {
        int startIndex = 0;
        int resultsPerPage = 2000;
        bool hasMoreData = true;

        while (hasMoreData)
        {
            // 1. Build the paginated URL
            var url = $"https://services.nvd.nist.gov/rest/json/cves/2.0/?resultsPerPage={resultsPerPage}&startIndex={startIndex}";
            
            // 2. Fetch the chunk of 2,000 CVEs
            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadFromJsonAsync<NvdResponse>();
            
            // 3. Save this chunk to your local database here...
            //SaveToDatabase(data.Vulnerabilities);
            
            // 4. Check if we've reached the end
            if (startIndex + resultsPerPage >= data.TotalResults)
            {
                hasMoreData = false;
            }
            else
            {
                // Move the index forward for the next page
                startIndex += resultsPerPage;
                
                // 5. THE CRITICAL STEP: Sleep to respect the rate limit
                // Sleep for 6 seconds (ensures you never exceed 5 requests per 30s)
                await Task.Delay(6000); 
            }
        }
    }

    // TODO Update the database with data recently modified within 24 hours
    public async Task UpdateDatabase() {}
}