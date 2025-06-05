using System;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Infrastructure;

public class GraphService(GraphServiceClient graphServiceClient)
{
    public async Task<string> GetObjectIdByEmail(string email)
    {
        try
        {
            var users = await graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id"];
                    requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
                });

            if (users?.Value?.FirstOrDefault()?.Id == null)
            {
                throw new NullReferenceException($"No user found with email: {email}");
            }

            return users.Value.First().Id;
        }
        catch (ServiceException ex)
        {
            throw new ServiceException($"Failed to fetch object ID: {ex.Message}");
        }
    }
}