using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Square9APIHelperLibrary.DataTypes;
using System;
using System.Collections.Generic;
using System.Net;
using File = Square9APIHelperLibrary.DataTypes.File;

namespace Square9APIHelperLibrary
{
    public class Square9API
    {
        #region Variables
        /// <summary>
        /// Rest API Connection, used to make requests to the server
        /// </summary>
        private static RestClient ApiClient;
        /// <summary>
        /// The Default SQL Instance returned by the server
        /// Loaded everytime a new license is created 
        /// <see cref="GetDefault"/> <seealso cref="CreateLicense"/>
        /// </summary>
        public string Default;
        /// <summary>
        /// Cached license information, contains connection information
        /// <see cref="CreateLicense"/>
        /// </summary>
        public License License;
        #endregion

        #region System
        /// <summary>
        /// Creates a new connection to the Square9API
        /// </summary>
        /// <param name="endpoint">Must be the full API endpoint (http://localhost/Square9API/)</param>
        /// <param name="username">Username of the account to authenticate with</param>
        /// <param name="password">Password of the account to authenticate with</param>
        /// <returns>Nothing</returns>
        public Square9API(string endpoint, string username, string password)
        {
            ApiClient = new RestClient(endpoint)
            {
                Authenticator = new HttpBasicAuthenticator(username, password)
            };
        }

        /// <summary>
        /// Checks if the current authenticated user is an Administrator
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> IsAdminAsync()
        {
            var Request = new RestRequest("api/userAdmin/isAdmin");
            var Response = await ApiClient.ExecuteAsync<bool>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Loads default SQL Instance from the server
        /// <see cref="Default"/>
        /// </summary>
        /// <returns><see cref="string"/></returns>
        private async Task<string> GetDefaultAsync()
        {
            var Request = new RestRequest("api/admin/instances/default");
            var Response = await ApiClient.ExecuteAsync<string>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns the user currently logged in
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetUsernameAsync()
        {
            var Request = new RestRequest("api/admin");
            var Response = await ApiClient.ExecuteAsync<string>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Configures the outgoing mail server settings
        /// </summary>
        /// <param name="databaseId">The database you would like to update these settings for</param>
        /// <param name="emailServer"><see cref="EmailServer"/></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<EmailServer> UpdateEmailOptionsAsync(int databaseId, EmailServer emailServer)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/notifications", Method.Put);
            Request.AddJsonBody(emailServer);
            var Response = await ApiClient.ExecuteAsync<EmailServer>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while updating email options: {Response.Content}");
            }
            return JsonConvert.DeserializeObject<EmailServer>(Response.Content);
        }
        /// <summary>
        /// Gets the current outgoing mail server settings
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <returns><see cref="EmailServer"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<EmailServer> GetEmailOptionsAsync(int databaseId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/notifications");
            var Response = await ApiClient.ExecuteAsync<EmailServer>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while retrieving email options: {Response.Content}");
            }
            return JsonConvert.DeserializeObject<EmailServer>(Response.Content);
        }
        /// <summary>
        /// Creates a new stamp on the server
        /// </summary>
        /// <param name="stamp"><see cref="Stamp"/></param>
        /// <returns><see cref="Stamp"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Stamp> CreateStampAsync(Stamp stamp)
        {
            var Request = new RestRequest($"api/admin/stamps", Method.Post);
            Request.AddJsonBody(stamp);
            var Response = await ApiClient.ExecuteAsync<Stamp>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred creating stamp: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns a list of stamps on the server
        /// </summary>
        /// <returns><see cref="Stamp"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Stamp>> GetStampsAsync()
        {
            var Request = new RestRequest($"api/admin/stamps");
            var Response = await ApiClient.ExecuteAsync<List<Stamp>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while retrieving stamps: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a stamp from the server
        /// </summary>
        /// <param name="stamp"><see cref="Stamp"/></param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteStampAsync(Stamp stamp)
        {
            var Request = new RestRequest($"api/admin/stamps/{stamp.Id}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred deleting stamp: {Response.Content}");
            }
        }
        /// <summary>
        /// Get the server's current registration status
        /// </summary>
        /// <returns><see cref="Registration"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Registration> GetRegistrationAsync()
        {
            var Request = new RestRequest($"api/admin/registration");
            var Response = await ApiClient.ExecuteAsync<Registration>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while retrieving registration: {Response.Content}");
            }
            Registration registration = Response.Data;
            var FeatureRequest = new RestRequest($"api/admin/registration?features=true", Method.Get);
            var FeatureResponse = await ApiClient.ExecuteAsync(FeatureRequest);
            if (FeatureResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while retrieving registration: {FeatureResponse.Content}");
            }
            registration.Features = JsonConvert.DeserializeObject<IDictionary<string, string>>(FeatureResponse.Content);
            return registration;
        }
        /// <summary>
        /// Submits a request to Square 9 for registration data
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="serial"></param>
        /// <exception cref="Exception"></exception>
        public async Task RequestWebRegistrationAsync(string uniqueId, string serial)
        {
            Register register = new Register(uniqueId, serial);
            var Request = new RestRequest($"api/admin/registration", Method.Post);
            Request.AddJsonBody(register);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while requesting web registration: {Response.Content}");
            }
        }
        /// <summary>
        /// Manually registers the server
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="serial"></param>
        /// <param name="registration"></param>
        /// <exception cref="Exception"></exception>
        public async Task ManualRegistrationAsync(string uniqueId, string serial, string registration)
        {
            Register register = new Register(uniqueId, serial, registration);
            var Request = new RestRequest($"api/admin/registration", Method.Put);
            Request.AddJsonBody(register);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"An error occurred while performing manual registration: {Response.Content}");
            }
        }
        #endregion

        #region Licenses
        /// <summary>
        /// Requests a license from the server
        /// </summary>
        /// <returns><see cref="License"/></returns>
        public async Task<License> CreateLicenseAsync()
        {
            var Request = new RestRequest("api/licenses");
            var Response = await ApiClient.ExecuteAsync<License>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                if (Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Unable to get a License: The passed user is Unauthorized.");
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception($"Unable to get a License: {Response.Content}");
                }
                else if (Response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Unable to get a License: Unable to connect to the license server, server not found.");
                }
                else if (Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new Exception("Unable to get a License: 403 Forbidden.");
                }
                else if (Response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    throw new Exception("Unable to get a License: The Request Timed out.");
                }
                else
                {
                    throw new Exception("Unable to get a License: Please check your connection settings.");
                }
            }
            License = Response.Data;
            Default = await GetDefaultAsync();
            return Response.Data;
        }
        /// <summary>
        /// Requests for a license to be deleted from the server
        /// </summary>
        /// <param name="license">Must be a active license</param>
        public async Task DeleteLicenseAsync(License license = null)
        {
            if (license != null || License != null)
            {
                string token = (license != null) ? license.Token : License.Token;
                var Request = new RestRequest($"api/licenses/{token}");
                var Response = await ApiClient.ExecuteAsync(Request);
                if (Response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Unable to release license token: {Response.Content}");
                }
                License = null; //Delete cached license
            }
        }
        /// <summary>
        /// Requests a list of all licenses from the server
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<License>> GetLicensesAsync()
        {
            var Request = new RestRequest($"api/LicenseManager");
            var Response = await ApiClient.ExecuteAsync<List<License>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get licenses: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Releases a specific license from the server, can enforce logout with optional parameter
        /// </summary>
        /// <param name="license"><see cref="License"/></param>
        /// <param name="forceLogout">When true will delete license token from the server, forcing user to log back in</param>
        /// <exception cref="Exception"></exception>
        public async Task ReleaseLicenseAsync(License license, bool forceLogout = false)
        {
            var Request = new RestRequest($"api/LicenseManager?userToken={license.Token}&forceLogout={forceLogout}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to release license: {Response.Content}");
            }
        }
        /// <summary>
        /// Releases all licenses from the server, can enforce logout with optional parameter
        /// </summary>
        /// <param name="forceLogout">When true will delete license token from the server, forcing user to log back in</param>
        /// <exception cref="Exception"></exception>
        public async Task ReleaseAllLicensesAsync(bool forceLogout = false)
        {
            var Request = new RestRequest($"api/LicenseManager?All=true&forceLogout={forceLogout}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to release licenses: {Response.Content}");
            }
        }
        #endregion

        #region Database
        /// <summary>
        /// Requests a list of databases from the server
        /// </summary>
        /// <param name="databaseId">Optional: The ID of the database you would like to return in the list</param>
        /// <returns><see cref="DatabaseList"/></returns>
        public async Task<DatabaseList> GetDatabasesAsync(int databaseId = 0)
        {
            var Request = (databaseId >= 1) ? new RestRequest($"api/dbs/{databaseId}") : new RestRequest("api/dbs");
            var Response = await ApiClient.ExecuteAsync<DatabaseList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get databases: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of databases from the server using the admin api endpoint. Returns additional information for each database over the normal <see cref="GetDatabases(int)"/>
        /// </summary>
        /// <param name="databaseId">Optional: The ID of the database you would like to return in the list</param>
        /// <returns>List of <see cref="AdminDatabase"/></returns>
        public async Task<List<AdminDatabase>> GetAdminDatabasesAsync(int databaseId = 0)
        {
            var Request = (databaseId >= 1) ? new RestRequest($"api/admin/databases/{databaseId}") : new RestRequest("api/admin/databases");
            var Response = await ApiClient.ExecuteAsync<List<AdminDatabase>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get databases: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new database on the server
        /// </summary>
        /// <param name="database">The new database to be created on the server <see cref="NewAdminDatabase"/></param>
        /// <returns><see cref="AdminDatabase"/></returns>
        public async Task<AdminDatabase> CreateDatabaseAsync(AdminDatabase database)
        {
            var Request = new RestRequest($"api/admin/databases", Method.Post);
            if (database.Server == null) { database.Server = Default; }
            Request.AddJsonBody(database);
            var Response = await ApiClient.ExecuteAsync<AdminDatabase>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create database: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Updates a existing database on the server
        /// </summary>
        /// <param name="database">The existing database to be updates on the server <see cref="AdminDatabase"/></param>
        /// <returns><see cref="AdminDatabase"/></returns>
        public async Task<AdminDatabase> UpdateDatabaseAsync(AdminDatabase database)
        {
            var Request = new RestRequest($"api/admin/databases/{database.Id}", Method.Put);
            if (database.Server == null) { database.Server = Default; }
            Request.AddJsonBody(database);
            var Response = await ApiClient.ExecuteAsync<AdminDatabase>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update database: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a existing database on the server
        /// </summary>
        /// <param name="databaseId">The ID of the database to be deleted <see cref="Database.Id"/></param>
        /// <param name="drop">Optional: Determines if the database should be dropped from SQL, set to false by default</param>
        public async Task DeleteDatabaseAsync(int databaseId, bool drop = false)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}?Drop={drop}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete database: {Response.Content}");
            }
        }
        /// <summary>
        /// Requests a SQL index rebuild on all tables withing the passed database
        /// </summary>
        /// <param name="databaseId">The ID of the database to be rebuilt <see cref="Database.Id"/></param>
        public async Task RebuildDatabaseIndexAsync(int databaseId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}?rebuild=true");
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to rebuild database index: {Response.Content}");
            }
        }
        #endregion

        #region Archive
        /// <summary>
        /// Requests a list of root archives in a database or sub archives in a archive from the server
        /// </summary>
        /// <param name="databaseId">The ID of the database you would like to return a list of archives from</param>
        /// <param name="archiveId">Optional: The ID of the archive you would like to return a list of archives from</param>
        /// <returns><see cref="ArchiveList"/></returns>
        public async Task<ArchiveList> GetArchivesAsync(int databaseId, int archiveId = 0)
        {
            var Request = (archiveId >= 1) ? new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}") : new RestRequest($"api/dbs/{databaseId}/archives");
            var Response = await ApiClient.ExecuteAsync<ArchiveList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get archives: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of archives in a database or sub archives in a archive from the server, includes additional admin only details
        /// </summary>
        /// <param name="databaseId">The ID of the database you would like to return a list of archives from</param>
        /// <param name="archiveId">Optional: The ID of the archive you would like to return a list of archvies from</param>
        /// <returns>List of <see cref="AdminArchive"/></returns>
        public async Task<List<AdminArchive>> GetAdminArchivesAsync(int databaseId, int archiveId = 0)
        {
            var Request = (archiveId >= 1) ? new RestRequest($"api/admin/databases/{databaseId}/archives/{archiveId}") : new RestRequest($"api/admin/databases/{databaseId}/archives");
            var Response = await ApiClient.ExecuteAsync<List<AdminArchive>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get archives: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Request a list of fields in a archive or sub archive from the server
        /// </summary>
        /// <param name="databaseId">The ID of the database the archive is in</param>
        /// <param name="archiveId">The ID of the archive to return fields from</param>
        /// <returns>List of <see cref="Field"/></returns>
        public async Task<List<Field>> GetArchiveFieldsAsync(int databaseId, int archiveId)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}?type=fields");
            var Response = await ApiClient.ExecuteAsync<List<Field>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get archive fields: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new archive on the server
        /// </summary>
        /// <param name="databaseId">The database id where the archive should be created</param>
        /// <param name="archive">The archive to be created</param>
        /// <returns><see cref="AdminArchive"/></returns>
        public async Task<AdminArchive> CreateArchiveAsync(int databaseId, NewAdminArchive archive)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/archives", Method.Post);
            Request.AddJsonBody(archive);
            var Response = await ApiClient.ExecuteAsync<AdminArchive>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create archive: {Response.Content} \n {JsonConvert.SerializeObject(archive)}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a archive on the server
        /// </summary>
        /// <param name="databaseId">Database ID that relates to the archive</param>
        /// <param name="archiveId">Archive ID of the archive to be deleted</param>
        public async Task DeleteArchiveAsync(int databaseId, int archiveId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/archives/{archiveId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete archive: {Response.Content}");
            }
        }
        /// <summary>
        /// Edit the properties of a archive
        /// </summary>
        /// <param name="databaseId">Database ID that relates to the archive</param>
        /// <param name="archive">Archive to be updated</param>
        /// <returns></returns>
        public async Task<AdminArchive> UpdateArchiveAsync(int databaseId, AdminArchive archive)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/archives/{archive.Id}", Method.Put);
            Request.AddJsonBody(archive);
            var Response = await ApiClient.ExecuteAsync<AdminArchive>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update archive: {Response.Content} \n {JsonConvert.SerializeObject(archive)}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests Global Archive Options from the specified database on the server
        /// </summary>
        /// <param name="databaseId">ID of the database to request options from</param>
        /// <returns><see cref="GlobalArchiveOptions"/></returns>
        public async Task<GlobalArchiveOptions> GetGlobalArchiveOptionsAsync(int databaseId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/options/archives");
            var Response = await ApiClient.ExecuteAsync<GlobalArchiveOptions>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get GlobalArchiveOptions: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Updates the Global Archive Options for the specified database on the server
        /// </summary>
        /// <param name="databaseId">ID of the database to update options on</param>
        /// <param name="globalArchiveOptions">Options object to update</param>
        /// <returns><see cref="GlobalArchiveOptions"/></returns>
        public async Task<GlobalArchiveOptions> UpdateGlobalArchiveOptionsAsync(int databaseId, GlobalArchiveOptions globalArchiveOptions)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/options/archives", Method.Put);
            Request.AddJsonBody(globalArchiveOptions);
            var Response = await ApiClient.ExecuteAsync<GlobalArchiveOptions>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update GlobalArchiveOptions: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Send a number of documents to the queue to be run through content indexing
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="archiveId">Archive ID</param>
        /// <param name="count">Number of most recent documents to index</param>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> RebuildArchiveContentIndexAsync(int databaseId, int archiveId, int count = 1000)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/archives/{archiveId}/indexrebuild?docCount={count}");
            var Response = await ApiClient.ExecuteAsync<bool>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to rebuild archive content index: {Response.Content}");
            }
            return Response.Data;
        }
        #endregion

        #region Search
        /// <summary>
        /// Requests a list of searches from the server
        /// Can be used to request a list of all searches from a database, archive or an individual search
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="archiveId">Optional: Archive ID</param>
        /// <param name="searchId">Optional: Search ID</param>
        /// <returns><see cref="Search"/></returns>
        public async Task<List<Search>> GetSearchesAsync(int databaseId, int archiveId = 0, int searchId = 0)
        {
            var Request = (searchId >= 1) ? new RestRequest($"api/dbs/{databaseId}/searches/{searchId}") : (archiveId >= 1) ? new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/searches") : new RestRequest($"api/dbs/{databaseId}/searches");
            var Response = await ApiClient.ExecuteAsync<List<Search>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get searches: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns results from a single search
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="search">The Search you'd like to return results for <see cref="Search"/></param>
        /// <param name="page">Optional: Denotes the specific page or results returned (Default is 1)</param>
        /// <param name="recordsPerPage">Optional: Denotes how many records should be returned per page (Default is 50)</param>
        /// <param name="tabId">Optional: Returns results from a specific view tab</param>
        /// <param name="sort">Optional: Sorts results based on desired column</param>
        /// <param name="time">Optional: Epoch time stamp</param>
        /// <returns><see cref="Result"/></returns>
        public async Task<Result> GetSearchResultsAsync(int databaseId, Search search, int page = 0, int recordsPerPage = 0, int tabId = 0, int sort = 0, int time = 0)
        {
            //Format Search Criteria
            List<string> searchCriteria = new List<string>();
            foreach (SearchDetail criteria in search.Detail)
            {
                if (criteria.Val != "")
                {
                    searchCriteria.Add($"{criteria.Id}:\"{criteria.Val}\"");
                }
            }
            string pageParam = (page >= 1) ? $"&Page={page}" : "";
            string recordsPerPageParam = (recordsPerPage >= 1) ? $"&RecordsPerPage={recordsPerPage}" : "";
            string tabIdParam = (tabId >= 1) ? $"&tabId={tabId}" : "";
            string sortParam = (sort >= 1) ? $"&Sort={sort}" : "";
            string timeParam = (time >= 1) ? $"&time={time}" : "";
            var Request = new RestRequest($"api/dbs/{databaseId}/searches/{search.Id}/archive/{search.Parent}/documents?SecureId={search.Hash}&SearchCriteria={{{string.Join(",", searchCriteria)}}}{pageParam}{recordsPerPageParam}{tabIdParam}{sortParam}{timeParam}&Count=false");
            var Response = await ApiClient.ExecuteAsync<Result>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get search results: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns count from a single search
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="search">The Search you'd like to return results for <see cref="Search"/></param>
        /// <param name="page">Optional: Denotes the specific page or results returned (Default is 1)</param>
        /// <param name="recordsPerPage">Optional: Denotes how many records should be returned per page (Default is 50)</param>
        /// <param name="tabId">Optional: Returns results from a specific view tab</param>
        /// <param name="sort">Optional: Sorts results based on desired column</param>
        /// <param name="time">Optional: Epoch time stamp</param>
        /// <returns><see cref="ArchiveCount"/></returns>
        public async Task<ArchiveCount> GetSearchCountAsync(int databaseId, Search search, int page = 0, int recordsPerPage = 0, int tabId = 0, int sort = 0, int time = 0)
        {
            //Format Search Criteria
            List<string> searchCriteria = new List<string>();
            foreach (SearchDetail criteria in search.Detail)
            {
                if (criteria.Val != "")
                {
                    searchCriteria.Add($"{criteria.Id}:\"{criteria.Val}\"");
                }
            }
            string pageParam = (page >= 1) ? $"&Page={page}" : "";
            string recordsPerPageParam = (recordsPerPage >= 1) ? $"&RecordsPerPage={recordsPerPage}" : "";
            string tabIdParam = (tabId >= 1) ? $"&tabId={tabId}" : "";
            string sortParam = (sort >= 1) ? $"&Sort={sort}" : "";
            string timeParam = (time >= 1) ? $"&time={time}" : "";
            var Request = new RestRequest($"api/dbs/{databaseId}/searches/{search.Id}/archive/{search.Parent}/documents?SecureId={search.Hash}&SearchCriteria={{{string.Join(",", searchCriteria)}}}{pageParam}{recordsPerPageParam}{tabIdParam}{sortParam}{timeParam}&Count=true");
            var Response = await ApiClient.ExecuteAsync<ArchiveCount>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get search count: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new search on the server
        /// </summary>
        /// <param name="databaseId">The database id where the search should be created</param>
        /// <param name="search">The search to be created</param>
        /// <returns><see cref="AdminSearch"/></returns>
        public async Task<AdminSearch> CreateSearchAsync(int databaseId, NewAdminSearch search)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/searches", Method.Post);
            Request.AddJsonBody(search);
            var Response = await ApiClient.ExecuteAsync<AdminSearch>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create search: {Response.Content} \n {JsonConvert.SerializeObject(search)}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a search on the server
        /// </summary>
        /// <param name="databaseId">Database ID that relates to the search</param>
        /// <param name="searchId">Search ID of the search to be deleted</param>
        public async Task DeleteSearchAsync(int databaseId, int searchId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/searches/{searchId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete search: {Response.Content}");
            }
        }
        /// <summary>
        /// Edit the properties of a archive
        /// </summary>
        /// <param name="databaseId">Database ID that relates to the archive</param>
        /// <param name="search">Archive to be updated</param>
        /// <returns></returns>
        public async Task<AdminSearch> UpdateSearchAsync(int databaseId, AdminSearch search)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/searches/{search.Id}", Method.Put);
            Request.AddJsonBody(search);
            var Response = await ApiClient.ExecuteAsync<AdminSearch>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update search: {Response.Content} \n {JsonConvert.SerializeObject(search)}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of searches from the server
        /// Can be used to request a list of all searches from a database, or individual search
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="searchId">Optional: Search ID</param>
        /// <returns><see cref="AdminSearch"/></returns>
        public async Task<List<AdminSearch>> GetAdminSearchesAsync(int databaseId, int searchId = 0)
        {
            var Request = (searchId >= 1) ? new RestRequest($"api/admin/databases/{databaseId}/searches/{searchId}") : new RestRequest($"api/admin/databases/{databaseId}/searches");
            var Response = await ApiClient.ExecuteAsync<List<AdminSearch>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get searches: {Response.Content}");
            }
            return Response.Data;
        }
        #endregion

        #region Inbox
        /// <summary>
        /// Requests a list of inboxes from the server
        /// </summary>
        /// <returns><see cref="Inbox"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<InboxList> GetInboxesAsync()
        {
            var Request = new RestRequest($"api/inboxes");
            var Response = await ApiClient.ExecuteAsync<InboxList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get inboxes: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a single specified inbox from the server
        /// </summary>
        /// <param name="inboxId">The ID of the desired inbox</param>
        /// <returns><see cref="Inbox"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Inbox> GetInboxAsync(int inboxId)
        {
            var Request = new RestRequest($"api/inboxes/{inboxId}");
            var Response = await ApiClient.ExecuteAsync<Inbox>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get inbox: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of admin inboxes from the server
        /// </summary>
        /// <returns><see cref="AdminInbox"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<AdminInbox>> GetAdminInboxesAsync()
        {
            var Request = new RestRequest($"api/admin/inboxes");
            var Response = await ApiClient.ExecuteAsync<List<AdminInbox>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get admin inbox: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns a specified inboxes security objects
        /// </summary>
        /// <param name="inboxId">The ID of the desired Inbox</param>
        /// <returns><see cref="Security"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Security>> GetAdminInboxSecurityAsync(int inboxId)
        {
            var Request = new RestRequest($"api/admin/inboxes/{inboxId}");
            var Response = await ApiClient.ExecuteAsync<List<Security>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get admin inbox security: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests the GlobalInbox options for the server
        /// </summary>
        /// <returns><see cref="GlobalInboxOptions"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<GlobalInboxOptions> GetGlobalInboxOptionsAsync()
        {
            var Request = new RestRequest($"api/admin/options/inboxes");
            var Response = await ApiClient.ExecuteAsync<GlobalInboxOptions>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get global inbox options: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Updates the global inbox options on the server
        /// </summary>
        /// <param name="option">The modified <see cref="GlobalInboxOptions"/></param>
        /// <returns><see cref="GlobalInboxOptions"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<GlobalInboxOptions> UpdateGlobalInboxOptionsAsync(GlobalInboxOptions option)
        {
            var Request = new RestRequest($"api/admin/options/inboxes", Method.Put);
            Request.AddJsonBody(option);
            var Response = await ApiClient.ExecuteAsync<GlobalInboxOptions>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update Global Inbox Options: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new inbox on the server
        /// </summary>
        /// <param name="inbox">The new <see cref="NewAdminInbox"/></param>
        /// <returns><see cref="AdminInbox"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminInbox> CreateInboxAsync(NewAdminInbox inbox)
        {
            var Request = new RestRequest($"api/admin/inboxes", Method.Post);
            Request.AddJsonBody(inbox);
            var Response = await ApiClient.ExecuteAsync<AdminInbox>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create inbox: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a specified inbox from the server
        /// </summary>
        /// <param name="inboxId">The Id of the desired Inbox</param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteInboxAsync(int inboxId)
        {
            var Request = new RestRequest($"api/admin/inboxes/{inboxId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete inbox: {Response.Content}");
            }
        }
        #endregion

        #region Field
        /// <summary>
        /// Returns a list of fields in a given database
        /// </summary>
        /// <param name="databaseId">datbase ID</param>
        /// <param name="fieldId">Optional: Field ID to return</param>
        /// <returns><see cref="AdminField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<AdminField>> GetFieldsAsync(int databaseId, int fieldId = 0)
        {
            var Request = (fieldId >= 1) ? new RestRequest($"api/admin/databases/{databaseId}/fields/{fieldId}") : new RestRequest($"api/admin/databases/{databaseId}/fields");
            var Response = await ApiClient.ExecuteAsync<List<AdminField>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get fields: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Create a new field on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="field">New Field to be created</param>
        /// <returns><see cref="AdminField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminField> CreateFieldAsync(int databaseId, NewAdminField field)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/fields", Method.Post);
            Request.AddJsonBody(field);
            var Response = await ApiClient.ExecuteAsync<AdminField>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create field: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Update the passed in field on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="field">Field to be updated</param>
        /// <returns><see cref="AdminField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminField> UpdateFieldAsync(int databaseId, AdminField field)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/fields/{field.Id}", Method.Put);
            Request.AddJsonBody(field);
            var Response = await ApiClient.ExecuteAsync<AdminField>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update field: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a given field on the database
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="fieldId">Field ID</param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteFieldAsync(int databaseId, int fieldId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/fields/{fieldId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable delete field: {Response.Content}");
            }
        }
        /// <summary>
        /// Requests a individual table field from the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="fieldId">Table Field ID</param>
        /// <returns><see cref="AdminTableField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminTableField> GetTableFieldAsync(int databaseId, int fieldId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/tablefields/{fieldId}");
            var Response = await ApiClient.ExecuteAsync<AdminTableField>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get table field: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of all table fields in a given database
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <returns><see cref="AdminTableField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<AdminTableField>> GetTableFieldsAsync(int databaseId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/tablefields");
            var Response = await ApiClient.ExecuteAsync<List<AdminTableField>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get table fields: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new table field in a given database on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="field">Field ID</param>
        /// <returns><see cref="AdminTableField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminTableField> CreateTableFieldAsync(int databaseId, NewAdminTableField field)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/tablefields", Method.Post);
            Request.AddJsonBody(field);
            var Response = await ApiClient.ExecuteAsync<AdminTableField>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create table field: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Updates a table field on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="field">Field ID</param>
        /// <returns><see cref="AdminTableField"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminTableField> UpdateTableFieldAsync(int databaseId, AdminTableField field)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/tablefields/{field.Id}", Method.Put);
            Request.AddJsonBody(field);
            var Response = await ApiClient.ExecuteAsync<AdminTableField>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update table field: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes the specified table field from the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="fieldId">Field ID</param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteTableFieldAsync(int databaseId, int fieldId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/tablefields/{fieldId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable delete table field: {Response.Content}");
            }
        }
        /// <summary>
        /// Requests a individual list from the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="listId">Field ID</param>
        /// <returns><see cref="AdminList"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminList> GetListAsync(int databaseId, int listId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists/{listId}");
            var Response = await ApiClient.ExecuteAsync<AdminList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get list: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a list of lists from the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <returns><see cref="AdminList"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<AdminList>> GetListsAsync(int databaseId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists");
            var Response = await ApiClient.ExecuteAsync<List<AdminList>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get lists: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Creates a new list on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="list">The new <see cref="AdminList"/></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminList> CreateListAsync(int databaseId, NewAdminList list)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists", Method.Post);
            Request.AddJsonBody(list);
            var Response = await ApiClient.ExecuteAsync<AdminList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create list: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Updates a list on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="list">List object to update</param>
        /// <returns><see cref="AdminList"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminList> UpdateListAsync(int databaseId, AdminList list)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists/{list.Id}", Method.Put);
            Request.AddJsonBody(list);
            var Response = await ApiClient.ExecuteAsync<AdminList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update list: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Deletes a list on the server
        /// </summary>
        /// <param name="databaseId">Database ID</param>
        /// <param name="listId">List ID to be deleted</param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteListAsync(int databaseId, int listId)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists/{listId}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable delete list: {Response.Content}");
            }
        }
        /// <summary>
        /// Loads an assembly lists values into the list object
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AdminList> LoadAssemblyListAsync(int databaseId, AdminList list)
        {
            var Request = new RestRequest($"api/admin/databases/{databaseId}/lists", Method.Put);
            Request.AddJsonBody(list);
            var Response = await ApiClient.ExecuteAsync<AdminList>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get assembly list: {Response.Content}");
            }
            return Response.Data;
        }
        #endregion

        #region Document
        /// <summary>
        /// Requests an individual document from the server based on ID
        /// </summary>
        /// <param name="databaseId">DatabaseID</param>
        /// <param name="archiveId">ArchiveID</param>
        /// <param name="documentId">Document ID</param>
        /// <exception cref="Exception"></exception>
        public async Task<Result> GetArchiveDocumentAsync(int databaseId, int archiveId, int documentId)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}?DocumentID={documentId}&Token={License.Token}");
            var SecureIDResponse = await ApiClient.ExecuteAsync(Request);
            if (SecureIDResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document Secure ID: {SecureIDResponse.Content}");
            }
            string SecureID = SecureIDResponse.Content;
            Console.WriteLine(SecureID);
            var DocRequest = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{documentId}?SecureId={SecureID}");
            var Response = await ApiClient.ExecuteAsync<Result>(DocRequest);
            if (SecureIDResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests all meta data for a given document
        /// </summary>
        /// <param name="databaseId">DatabaseID</param>
        /// <param name="archiveId">ArchiveID</param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <returns>Returns the same <see cref="Result"/> that <see cref="GetSearchResults(int, Search, int, int, int, int, int)"/> would return</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Result> GetArchiveDocumentMetaDataAsync(int databaseId, int archiveId, Doc document)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}?SecureId={document.Hash}");
            var Response = await ApiClient.ExecuteAsync<Result>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document Meta Data: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Requests a files details from the server for a specified inbox/file
        /// </summary>
        /// <param name="inboxId">The ID of the inbox the file resides in</param>
        /// <param name="fileName">The original filename of the file to return details on</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DocDetails> GetInboxDocumentDetailsAsync(int inboxId, File file)
        {
            var Request = new RestRequest($"api/inboxes?FilePath={file.FileName}{file.FileType}&Id={inboxId}");
            var Response = await ApiClient.ExecuteAsync<DocDetails>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document Details: {Response.Content}");
            }
            return JsonConvert.DeserializeObject<DocDetails>(Response.Content);
        }
        /// <summary>
        /// Downloads a given documents file to either a temp path or specific local path
        /// </summary>
        /// <param name="databaseId">DatabaseID</param>
        /// <param name="archiveId">ArchiveID</param>
        /// <param name="document"><see cref="Doc"/> to be downloaded from the server</param>
        /// <param name="savePath">Optional: Local path to save the documents file to</param>
        /// <returns>A <see cref="string"/> of the downloaded files filepath</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetArchiveDocumentFileAsync(int databaseId, int archiveId, Doc document, string savePath = "")
        {
            var fileName = (savePath == "") ? $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}{document.FileType}" : $"{savePath}{document.Id}{document.FileType}";
            var writer = System.IO.File.OpenWrite(fileName);
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/file?SecureId={document.Hash}") {
                ResponseWriter = responseStream => {
                    using (responseStream)
                    {
                        responseStream.CopyTo(writer);
                    }
                    return responseStream;
                }
            };
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to download file: {Response.Content}");
            }
            return fileName;
        }
        /// <summary>
        /// Requests a documents file from a inbox on the server
        /// </summary>
        /// <param name="inboxId">The target inboxes ID</param>
        /// <param name="file"><see cref="File"/></param>
        /// <param name="savePath">Optional: Local path to save file</param>
        /// <returns>A <see cref="string"/> containing the local files path</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetInboxDocumentFileAsync(int inboxId, File file, string savePath = "")
        {
            var fileName = (savePath == "") ? $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}{file.FileType}" : $"{savePath}{file.FileName}{file.FileType}";
            var writer = System.IO.File.OpenWrite(fileName);
            var Request = new RestRequest($"api/inboxes/{inboxId}?FileName={file.FileName}{file.FileType}") {
                ResponseWriter = responseStream =>
                {
                    using (responseStream)
                    {
                        responseStream.CopyTo(writer);
                    }
                    return responseStream;
                }
            };
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to download file: {Response.Content}");
            }
            return fileName;
        }
        /// <summary>
        /// Downloads a thumbnail of the document in JPG format
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <param name="savePath">The local filepath where you would like to save the thumbnail</param>
        /// <param name="height"><see cref="int"/> height in pixels the thumbnail should be downloaded in</param>
        /// <param name="width"><see cref="int"/> width in pixels the thumbnail should be downloaded in</param>
        /// <returns>a <see cref="string"/> containing the local file path of the downloaded thumbnail</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetArchiveDocumentThumbnailAsync(int databaseId, int archiveId, Doc document, string savePath = "", int height = 0, int width = 0)
        {
            var fileName = (savePath == "") ? $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.jpg" : $"{savePath}{document.Id}.jpg";
            var writer = System.IO.File.OpenWrite(fileName);
            var Request = (height == 0 || width == 0) ? new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/thumb?SecureId={document.Hash}") : new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/thumb?SecureId={document.Hash}&height={height}&width={width}")
            {
                ResponseWriter = responseStream =>
                {
                    using (responseStream)
                    {
                        responseStream.CopyTo(writer);
                    }
                    return responseStream;
                }
            };
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to download thumbnail: {Response.Content}");
            }
            return fileName;
        }
        /// <summary>
        /// Updates a documents index data on the server
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="document">Document to update on the server</param>
        /// <returns><see cref="Doc"/> *Document object returned is the same one passed to the method, the server returns nothing*</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Doc> UpdateDocumentIndexDataAsync(int databaseId, int archiveId, Doc document)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/save?SecureId={document.Hash}", Method.Post);
            DocumentIndexData IndexData = new DocumentIndexData();
            IndexData.IndexData.IndexFields = document.Fields;
            Request.AddJsonBody(IndexData);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update document index data: {Response.Content}");
            }
            return document;
        }
        /// <summary>
        /// Uploads a file to the server
        /// </summary>
        /// <param name="fileName">The full local path of the file to be uploaded</param>
        /// <returns><see cref="UploadedFiles"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<UploadedFiles> UploadDocumentAsync(string fileName)
        {
            var Request = new RestRequest($"api/files", Method.Post);
            Request.AddFile("File", fileName);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to upload file: {Response.Content}");
            }
            return JsonConvert.DeserializeObject<UploadedFiles>(Response.Content);
        }
        /// <summary>
        /// Imports a uploaded document into a archive
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="newFile"><see cref="NewFile"/></param>
        /// <exception cref="Exception"></exception>
        public async Task ImportArchiveDocumentAsync(int databaseId, int archiveId, NewFile newFile, bool useViewerCache = false)
        {
            var Request = new RestRequest((!useViewerCache) ? $"api/dbs/{databaseId}/archives/{archiveId}" : $"api/dbs/{databaseId}/archives/{archiveId}?useViewerCache=true", Method.Post);
            Request.AddJsonBody(newFile);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to index document: {Response.Content}");
            }
        }
        /// <summary>
        /// Imports a new document into a inbox
        /// </summary>
        /// <param name="inboxId">ID of the inbox to import the file to</param>
        /// <param name="file"><see cref="FileDetails"/> Returned by <see cref="UploadDocument(string)"/></param>
        /// <exception cref="Exception"></exception>
        public async Task ImportInboxDocumentAsync(int inboxId, FileDetails file)
        {
            var Request = new RestRequest($"api/inboxes/{inboxId}?FilePath={file.Name}&newFileName={file.OriginalName}", Method.Post);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to import document: {Response.Content}");
            }
        }
        /// <summary>
        /// Indexes a document from a inbox to a archive
        /// </summary>
        /// <param name="databaseId">Database ID of the destination database</param>
        /// <param name="archiveId">Archive ID of the desitination archive</param>
        /// <param name="inboxId">Inbox ID of the source inbox</param>
        /// <param name="file"><see cref="File"/></param>
        /// <param name="fields">Optional: <see cref="FileField"/></param>
        public async Task IndexInboxDocumentAsync(int databaseId, int archiveId, int inboxId, File file, List<FileField> fields = null)
        {
            DocDetails details = await GetInboxDocumentDetailsAsync(inboxId, file);

            NewFile newFile = new NewFile();
            if (fields != null) { newFile.Fields = fields; }
            FileDetails newFileDetails = new FileDetails();
            newFileDetails.Name = details.FileName;
            newFile.Files.Add(newFileDetails);

            await ImportArchiveDocumentAsync(databaseId, archiveId, newFile, true);
        }
        /// <summary>
        /// Downloads a zip file containing the specified documents
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="fieldId"><see cref="Doc.Fields"/></param>
        /// <param name="files"><see cref="FileExport"/></param>
        /// <param name="savePath">Optional: Path to save Zip file to</param>
        /// <returns><see cref="string"/> containing the local path of the downloaded zip file</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> ExportDocumentAsync(int databaseId, int fieldId, List<FileExport> files, string savePath = "")
        {
            var CreateExportJobRequest = new RestRequest($"api/dbs/{databaseId}/export?alwaysExportZip=true&auditEntry=Document+Exported&field={fieldId}", Method.Post);
            CreateExportJobRequest.AddJsonBody(files);
            var CreateExportJobResponse = await ApiClient.ExecuteAsync(CreateExportJobRequest);
            if (CreateExportJobResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create export job: {CreateExportJobResponse.Content}");
            }
            var fileName = (savePath == "") ? $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.zip" : $"{savePath}{CreateExportJobResponse.Content.Replace("\"", "")}.zip";
            var writer = System.IO.File.OpenWrite(fileName);
            Console.WriteLine($"api/dbs/{databaseId}/export?jobid={CreateExportJobResponse.Content.Replace("\"", "")}");
            var GetJobZipRequest = new RestRequest($"api/dbs/{databaseId}/export?jobid={CreateExportJobResponse.Content.Replace("\"", "")}")
            {
                ResponseWriter = responseStream =>
                {
                    using (responseStream)
                    {
                        responseStream.CopyTo(writer);
                    }
                    return responseStream;
                }
            };
            var GetJobZipResponse = await ApiClient.ExecuteAsync(GetJobZipRequest);
            if (GetJobZipResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to download export zip: {GetJobZipResponse.Content}");
            }
            return fileName;
        }
        /// <summary>
        /// Deletes a document from the server
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteArchiveDocumentAsync(int databaseId, int archiveId, Doc document)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/delete?SecureId={document.Hash}");
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable delete document: {Response.Content}");
            }
        }
        /// <summary>
        /// Deletes the specified document from a inbox
        /// </summary>
        /// <param name="inboxId">The ID of the inbox you would like to delete from</param>
        /// <param name="file"><see cref="File"/> to be deleted</param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteInboxDocumentAsync(int inboxId, File file)
        {
            var Request = new RestRequest($"api/inboxes/{inboxId}?FilePath={file.FileName}{file.FileType}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable delete document: {Response.Content}");
            }
        }
        /// <summary>
        /// Performs either a move or copy operation to a given document on the server
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/> The source archive ID</param>
        /// <param name="destArchiveId"><see cref="Archive.Id"/> The destination archive ID</param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <param name="move"><see cref="bool"/> Flag to perform move operation (Cut/Paste)</param>
        /// <returns><see cref="int"/> Doc ID of the moved/copied document</returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> TransferArchiveDocumentAsync(int databaseId, int archiveId, int destArchiveId, Doc document, bool move = false)
        {
            string type = (move) ? "move" : "copy";
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/{type}?DestinationArchive={destArchiveId}&SecureID={document.Hash}");
            var Response = await ApiClient.ExecuteAsync<int>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to transfer document: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Performs either a move or copy operation to a given document on the server
        /// </summary>
        /// <param name="inboxId">Inbox ID</param>
        /// <param name="destInboxId">Destination inbox ID</param>
        /// <param name="file"><see cref="File"/> to by transfered</param>
        /// <param name="move">Optional: When set to true, operation will delete the source file in the source inbox after copy</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> TransferInboxDocumentAsync(int inboxId, int destInboxId, File file, bool move = false)
        {
            var Request = new RestRequest($"api/inboxes/{inboxId}?FilePath={file.FileName}{file.FileType}&deleteOriginal={move}&targetInboxId={destInboxId}");
            var Response = await ApiClient.ExecuteAsync<string>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to transfer document: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns a list of document revisions from the server
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="documentId"><see cref="Doc.Id"/></param>
        /// <returns><see cref="Revision"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Revision>> GetDocumentRevisionsAsync(int databaseId, int archiveId, Doc document)
        {
            var Request = new RestRequest($"api/dbs/{databaseId}/archives/{archiveId}/documents/{document.Id}/rev?SecureId={document.Hash}");
            var Response = await ApiClient.ExecuteAsync<List<Revision>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document revisions: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns a queue object that the document is in 
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <returns><see cref="Queue"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Queue> GetDocumentQueueAsync(int databaseId, int archiveId, Doc document)
        {
            var Request = new RestRequest($"api/useraction?Database={databaseId}&Archive={archiveId}&Document={document.Id}&SecureId={document.Hash}");
            var Response = await ApiClient.ExecuteAsync<Queue>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get document queue: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Triggers a GlobalAction action on a given document
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="document"><see cref="Doc"/></param>
        /// <param name="action"><see cref="DataTypes.Action"/></param>
        /// <exception cref="Exception"></exception>
        public async Task FireDocumentQueueActionAsync(int databaseId, int archiveId, Doc document, DataTypes.Action action)
        {
            var Request = new RestRequest($"api/useraction?Database={databaseId}&Archive={archiveId}&Document={document.Id}&ActionId={action.Key}&SecureId={document.Hash}", Method.Post);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to trigger document action: {Response.Content}");
            }
        }
        #endregion

        #region Administration
        /// <summary>
        /// Returns a list of all users and groups that are secured to any database
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<SecuredGroup>> GetSecuredUsersAndGroupsAsync()
        {
            var Request = new RestRequest($"api/userAdmin/secured");
            var Response = await ApiClient.ExecuteAsync<List<SecuredGroup>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get secured users and groups: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns a list of all users and groups that are not secured to any database
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<UnsecuredGroup>> GetUnsecuredUsersAndGroupsAsync()
        {
            var Request = new RestRequest($"api/userAdmin/unsecured");
            var Response = await ApiClient.ExecuteAsync<List<UnsecuredGroup>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get unsecured users and groups: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns an object containing databases, their associated archives, and searches
        /// </summary>
        /// <returns><see cref="SecurityNode"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<SecurityNode>> GetTreeStructureAsync()
        {
            var Request = new RestRequest($"api/userAdmin/tree");
            var Response = await ApiClient.ExecuteAsync<List<SecurityNode>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get server tree view: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Returns specified user's archive permissions for a given archive
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="archiveId"><see cref="Archive.Id"/></param>
        /// <param name="username">Username to return permissions on</param>
        /// <returns><see cref="ArchivePermission"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ArchivePermission> GetUserArchivePermissionsAsync(int databaseId, int archiveId, string username)
        {
            var Request = new RestRequest($"api/userAdmin/archives?db={databaseId}&archive={archiveId}&username={username}");
            var Response = await ApiClient.ExecuteAsync<int>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get archive permissions: {Response.Content}");
            }
            return new ArchivePermission(Response.Data);
        }
        /// <summary>
        /// Returns specified user's inbox permissions for a given archive
        /// </summary>
        /// <param name="inbox"><see cref="Inbox.Id"/></param>
        /// <param name="username">Username to return permissions on</param>
        /// <returns><see cref="InboxPermission"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<InboxPermission> GetUserInboxPermissionsAsync(int inbox, string username)
        {
            var Request = new RestRequest($"api/userAdmin/inboxes?inbox={inbox}&username={username}");
            var Response = await ApiClient.ExecuteAsync<int>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get archive permissions: {Response.Content}");
            }
            return new InboxPermission(Response.Data);
        }
        /// <summary>
        /// Returns specified user's search properties for a given database
        /// </summary>
        /// <param name="databaseId"><see cref="Database.Id"/></param>
        /// <param name="username">Username to return search properties of</param>
        /// <returns><see cref="SearchProperties"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<SearchProperties>> GetUserSearchPropertiesAsync(int databaseId, string username)
        {
            var Request = new RestRequest($"api/userAdmin/searches?db={databaseId}&username={username}");
            var Response = await ApiClient.ExecuteAsync<List<SearchProperties>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Cannot get user search properties: {Response.Content}");
            }
            return Response.Data;
        }
        /// <summary>
        /// Sets the security for a given user/group on a given archive
        /// </summary>
        /// <param name="archiveSecurity"><see cref="ArchiveSecurity"/></param>
        /// <exception cref="Exception"></exception>
        public async Task SetArchiveSecurityAsync(ArchiveSecurity archiveSecurity)
        {
            var Request = new RestRequest($"api/userAdmin/archives", Method.Post);
            Request.AddJsonBody(archiveSecurity);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK || !Response.Content.Contains("\"Saved\":1"))
            {
                throw new Exception($"Unable to save archive security: {Response.Content}");
            }
        }
        /// <summary>
        /// Sets the security for a given user/group on a given inbox
        /// </summary>
        /// <param name="inboxSecurity"><see cref="InboxSecurity"/></param>
        /// <exception cref="Exception"></exception>
        public async Task SetInboxSecurityAsync(InboxSecurity inboxSecurity)
        {
            var Request = new RestRequest($"api/userAdmin/inboxes", Method.Post);
            Request.AddJsonBody(inboxSecurity);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to save inbox security: {Response.Content}");
            }
        }
        /// <summary>
        /// Sets the security for a given user/group on a given database
        /// </summary>
        /// <param name="databaseSecurity"><see cref="DatabaseSecurity"/></param>
        /// <exception cref="Exception"></exception>
        public async Task SetDatabaseSecurityAsync(DatabaseSecurity databaseSecurity)
        {
            var Request = new RestRequest($"api/userAdmin/databases", Method.Post);
            Request.AddJsonBody(databaseSecurity);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK || !Response.Content.Contains("\"Success\""))
            {
                throw new Exception($"Unable to save database security: {Response.Content}");
            }
        }
        /// <summary>
        /// Sets the security for a given user/group on a given search
        /// </summary>
        /// <param name="searchSecurity"><see cref="SearchSecurity"/></param>
        /// <exception cref="Exception"></exception>
        public async Task SetSearchSecurityAsync(SearchSecurity searchSecurity)
        {
            var Request = new RestRequest($"api/userAdmin/searches", Method.Post);
            Request.AddJsonBody(searchSecurity);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK || !Response.Content.Contains("\"Success\""))
            {
                throw new Exception($"Unable to save search security: {Response.Content}");
            }
        }
        /// <summary>
        /// Sets the search properties for a given user/group on a given search
        /// </summary>
        /// <param name="searchSecurity"><see cref="SearchSecurity"/></param>
        /// <exception cref="Exception"></exception>
        public async Task SetSearchPropertiesAsync(SearchSecurity searchSecurity)
        {
            var Request = new RestRequest($"api/userAdmin/searchType", Method.Post);
            Request.AddJsonBody(searchSecurity);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to save search properties: {Response.Content}");
            }
        }
        /// <summary>
        /// Creates a new S9 user on the server
        /// </summary>
        /// <param name="newUser"><see cref="User"/></param>
        /// <exception cref="Exception"></exception>
        public async Task CreateUserAsync(User newUser)
        {
            var Request = new RestRequest($"api/userAdmin/user?create=", Method.Post);
            Request.AddJsonBody(newUser);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create new user: {Response.Content}");
            }
        }
        /// <summary>
        /// Deletes a S9 user from the server
        /// </summary>
        /// <param name="user"><see cref="User"/></param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteUserAsync(User user)
        {
            var Request = new RestRequest($"api/userAdmin/user?name={user.Name}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete user: {Response.Content}");
            }
        }
        /// <summary>
        /// Updates a S9 user on the server
        /// </summary>
        /// <param name="user"><see cref="User"/></param>
        /// <exception cref="Exception"></exception>
        public async Task UpdateUserAsync(User user)
        {
            var Request = new RestRequest($"api/userAdmin/user?name={user.Name}", Method.Put);
            Request.AddJsonBody(user);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update user: {Response.Content}");
            }
        }
        /// <summary>
        /// Creates a new S9 group on the server
        /// </summary>
        /// <param name="newGroup"><see cref="Group"/></param>
        /// <exception cref="Exception"></exception>
        public async Task CreateGroupAsync(Group newGroup)
        {
            var Request = new RestRequest($"api/userAdmin/group?createGroup=", Method.Post);
            Request.AddJsonBody(newGroup);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to create new group: {Response.Content}");
            }
        }
        /// <summary>
        /// Deletes a S9 group from the server
        /// </summary>
        /// <param name="group"><see cref="Group"/></param>
        /// <exception cref="Exception"></exception>
        public async Task DeleteGroupAsync(Group group)
        {
            var Request = new RestRequest($"api/userAdmin/group?name={group.Name}", Method.Delete);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to delete group: {Response.Content}");
            }
        }
        /// <summary>
        /// Updates a S9 group on the server
        /// </summary>
        /// <param name="group"><see cref="Group"/></param>
        /// <exception cref="Exception"></exception>
        public async Task UpdateGroupAsync(Group group)
        {
            var Request = new RestRequest($"api/userAdmin/group?groupName={group.Name}", Method.Put);
            Request.AddJsonBody(group);
            var Response = await ApiClient.ExecuteAsync(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to update group: {Response.Content}");
            }
        }
        /// <summary>
        /// Requests a list of S9 groups and members on the server
        /// </summary>
        /// <returns><see cref="Group"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Group>> GetGroupsAsync()
        {
            var Request = new RestRequest($"api/userAdmin/s9groups");
            var Response = await ApiClient.ExecuteAsync<List<Group>>(Request);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Unable to get groups: {Response.Content}");
            }
            return Response.Data;
        }
        #endregion

    }
}
