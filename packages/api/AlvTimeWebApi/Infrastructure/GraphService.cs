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
    
    public async Task<string> GetProfilePictureBase64(string oid, string email)
    {
        try
        {
            var picture = await GetUserProfilePicture(oid: oid);
            return picture != null ? Convert.ToBase64String(picture) : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<byte[]> GetUserProfilePicture(string oid = null, string email = null)
    {
        try
        {
            // Determine which identifier to use
            string userId;
            if (!string.IsNullOrEmpty(oid))
            {
                userId = oid;
            }
            else if (!string.IsNullOrEmpty(email))
            {
                userId = await GetObjectIdByEmail(email);
            }
            else
            {
                throw new ArgumentException("Either oid or email must be provided");
            }

            // Fetch the profile photo
            var photoStream = await graphServiceClient.Users[userId].Photo.Content.GetAsync();
            
            if (photoStream == null)
            {
                return null;
            }

            using var memoryStream = new System.IO.MemoryStream();
            await photoStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 404)
        {
            // User has no profile picture
            return null;
        }
        catch (ServiceException ex)
        {
            throw new ServiceException($"Failed to fetch profile picture: {ex.Message}");
        }
    }
}