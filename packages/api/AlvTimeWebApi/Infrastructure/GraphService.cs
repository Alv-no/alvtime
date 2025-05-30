using System;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace AlvTimeWebApi.Infrastructure;

public class GraphService(GraphServiceClient graphServiceClient)
{
    public async Task<string> GetObjectIdByEmail(string email)
    {
        var users = await graphServiceClient.Users
            .GetAsync(requestConfiguration => 
            {
                requestConfiguration.QueryParameters.Select = ["id"];
                requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
            });

            Console.WriteLine("hei");
            // var user = users.FirstOrDefault();
            // if (user == null)
            // {
            //     return new Error(ErrorCodes.MissingEntity, $"User with email {email} not found in Entra ID");
            // }

            return "hehe";
        // catch (ServiceException ex)
        // {
        //     return new Error(ErrorCodes.AuthorizationError, $"Failed to fetch object ID: {ex.Message}");
        // }

    }
}