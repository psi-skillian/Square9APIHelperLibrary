using Microsoft.VisualStudio.TestTools.UnitTesting;
using Square9APIHelperLibrary;
using Square9APIHelperLibrary.DataTypes;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace APITest
{
    [TestClass]
    public class Square9APITests
    {
        #region Variables
        private string Endpoint = "http://10.0.0.220/Square9API";
        private string Username = "SSAdministrator";
        private string Password = "-------";
        #endregion

        #region System
        [TestMethod]
        [TestCategory("System")]
        public async Task BasicAPIConnectionAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            Console.WriteLine(JsonConvert.SerializeObject(await Connection.CreateLicenseAsync()));
            DatabaseList TestDatabaseList = await Connection.GetDatabasesAsync(1);
            await Connection.DeleteLicenseAsync();
            await Connection.DeleteLicenseAsync();
            Assert.AreEqual(1, TestDatabaseList.Databases[0].Id);
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task ReadDatabasesAndArchivesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<Database> Databases = (await Connection.GetDatabasesAsync()).Databases;
            Console.WriteLine($"{Databases[0].Id} : {Databases[0].Name}");
            List<Archive> Archives = (await Connection.GetArchivesAsync(Databases[0].Id)).Archives;
            Console.WriteLine($"    {Archives[0].Id} : {Archives[0].Name}");
            List<Archive> SubArchives = (await Connection.GetArchivesAsync(Databases[0].Id, Archives[0].Id)).Archives;
            Console.WriteLine($"        {SubArchives[0].Id} : {SubArchives[0].Name}");
            await Connection.DeleteLicenseAsync();
            Assert.IsTrue(SubArchives[0].Name != null);
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task CheckIfAdminAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Console.WriteLine(await Connection.IsAdminAsync());
            Assert.IsTrue(await Connection.IsAdminAsync());
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task GetUpdateEmailOptionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            EmailServer emailServer = await Connection.GetEmailOptionsAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(emailServer));

            emailServer.Auth.User = Username;
            Console.WriteLine(JsonConvert.SerializeObject(await Connection.UpdateEmailOptionsAsync(1, emailServer)));

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task GetCreateDeleteStampAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            List<Stamp> stamps = await Connection.GetStampsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(stamps));

            Stamp stamp = new Stamp();
            stamp.Text = "This is a test stamp 2";

            Stamp newStamp = await Connection.CreateStampAsync(stamp);
            Console.WriteLine(JsonConvert.SerializeObject(newStamp));

            await Connection.DeleteStampAsync(newStamp);

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task GetRegistrationAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            Registration registration = await Connection.GetRegistrationAsync();
            Console.WriteLine(JsonConvert.SerializeObject(registration));

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("System")]
        public async Task GetReleaseLicensesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            List<License> licenses = await Connection.GetLicensesAsync();
            Console.WriteLine(JsonConvert.SerializeObject(licenses));

            await Connection.ReleaseLicenseAsync(licenses[0]);

            await Connection.ReleaseAllLicensesAsync(true);

            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Database
        [TestMethod]
        [TestCategory("Database")]
        public async Task CreateUpdateDeleteDatabaseAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            AdminDatabase NewDatabase = new NewAdminDatabase("NewTestDatabase");
            AdminDatabase Database = await Connection.CreateDatabaseAsync(NewDatabase);
            Console.WriteLine(Database.Name);
            Database.Name = "NewDatabaseModified";
            Database = await Connection.UpdateDatabaseAsync(Database);
            Console.WriteLine(Database.Name);
            Console.WriteLine(Database.Id);
            await Connection.DeleteDatabaseAsync(Database.Id, true);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Database")]
        public async Task GetAdminDatabasesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminDatabase> Databases = await Connection.GetAdminDatabasesAsync();
            Console.WriteLine(JsonConvert.SerializeObject(Databases[0]));
            Databases = await Connection.GetAdminDatabasesAsync(Databases[0].Id);
            Console.WriteLine(Databases[0].Name);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Database")]
        public async Task RebuildDatabaseIndexAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            await Connection.RebuildDatabaseIndexAsync(1);
            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Archive
        [TestMethod]
        [TestCategory("Archive")]
        public async Task GetAdminArchivesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminArchive> Archives = await Connection.GetAdminArchivesAsync(1);
            Console.WriteLine(Archives[0].Name);
            Console.WriteLine($"    {(await Connection.GetAdminArchivesAsync(1, 2))[0].Name}");
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Archive")]
        public async Task GetArchiveFieldsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<Field> Fields = await Connection.GetArchiveFieldsAsync(1, 1);
            Field field = Fields[0];
            Console.WriteLine(Fields[2].Prop);
            FieldProp prop = new FieldProp(Fields[2].Prop);
            Console.WriteLine(JsonConvert.SerializeObject(prop));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Archive")]
        public async Task CreateUpdateDeleteArchiveAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminArchive archive = new NewAdminArchive("T2");
            archive.Fields = new List<int>() { 3 };
            archive.Parent = 3;
            AdminArchive newArchive = await Connection.CreateArchiveAsync(1, archive);
            newArchive.Name = "T1";
            AdminArchive updatedArchive = await Connection.UpdateArchiveAsync(1, newArchive);
            Console.WriteLine(updatedArchive.Name);
            await Connection.DeleteArchiveAsync(1, updatedArchive.Id);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Archive")]
        public async Task RebuildContentIndexAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Console.WriteLine(await Connection.RebuildArchiveContentIndexAsync(1, 3, 10000));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Archive")]
        public async Task GetUpdateGlobalArchiveOptionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            GlobalArchiveOptions options = await Connection.GetGlobalArchiveOptionsAsync(1);
            options.ShowAll = false;
            GlobalArchiveOptions updatedOptions = await Connection.UpdateGlobalArchiveOptionsAsync(1, options);
            Console.WriteLine(updatedOptions.ShowAll);
            updatedOptions.ShowAll = true;
            await Connection.UpdateGlobalArchiveOptionsAsync(1, updatedOptions);
            Console.WriteLine((await Connection.GetGlobalArchiveOptionsAsync(1)).ShowAll);
            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Search
        [TestMethod]
        [TestCategory("Search")]
        public async Task GetSearchesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<Search> searches = await Connection.GetSearchesAsync(1);
            List<Search> archiveSearches = await Connection.GetSearchesAsync(1, archiveId: 1);
            List<Search> search = await Connection.GetSearchesAsync(1, searchId: 6);
            Console.WriteLine(JsonConvert.SerializeObject(archiveSearches));
            Console.WriteLine(searches[0].Name);
            Console.WriteLine(archiveSearches[0].Name);
            Console.WriteLine(search[0].Name);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Search")]
        public async Task GetSearchResultsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            search.Detail[0].Val = "test";
            search.Detail[1].Val = "10/29/2020";
            Result results = await Connection.GetSearchResultsAsync(1, search);
            Console.WriteLine(JsonConvert.SerializeObject(results));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Search")]
        public async Task GetSearchCountAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<Search> search = await Connection.GetSearchesAsync(1, searchId: 20);
            Console.WriteLine(JsonConvert.SerializeObject(search));
            search[0].Detail[0].Val = "test";
            search[0].Detail[1].Val = "10/29/2020";
            Console.WriteLine(JsonConvert.SerializeObject(search));
            ArchiveCount results = await Connection.GetSearchCountAsync(1, search[0]);
            Console.WriteLine(JsonConvert.SerializeObject(results));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Search")]
        public async Task CreateUpdateDeleteSearchAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminSearch newSearch = new NewAdminSearch("New Test Search");
            newSearch.AddParameter(3, "contains", "Date:");
            newSearch.Parent = 3;
            newSearch.Archives.Add(3);
            AdminSearch search = await Connection.CreateSearchAsync(1, newSearch);
            Console.WriteLine(JsonConvert.SerializeObject(search));
            search.Name = "Newest Test Search";
            AdminSearch updatedSearch = await Connection.UpdateSearchAsync(1, search);
            Console.WriteLine(updatedSearch.Name);
            await Connection.DeleteSearchAsync(1, search.Id);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Search")]
        public async Task GetAdminSearchesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminSearch> searches = await Connection.GetAdminSearchesAsync(1, 6);
            Console.WriteLine(searches[0].Name);
            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Inbox
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task GetInboxesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            InboxList inboxes = await Connection.GetInboxesAsync();
            Console.WriteLine(JsonConvert.SerializeObject(inboxes));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task GetInboxAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Inbox inbox = await Connection.GetInboxAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(inbox));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task GetAdminInboxesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminInbox> inboxes = await Connection.GetAdminInboxesAsync();
            Console.WriteLine(JsonConvert.SerializeObject(inboxes));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task GetAdminInboxSecurityAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<Security> security = await Connection.GetAdminInboxSecurityAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(security));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task GetGlobalInboxOptionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            GlobalInboxOptions options = await Connection.GetGlobalInboxOptionsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(options));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task UpdateGlobalInboxOptionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            GlobalInboxOptions options = await Connection.GetGlobalInboxOptionsAsync();
            options.ShowAll = false;
            GlobalInboxOptions updatedOptions = await Connection.UpdateGlobalInboxOptionsAsync(options);
            Console.WriteLine(JsonConvert.SerializeObject(options));
            Console.WriteLine(JsonConvert.SerializeObject(updatedOptions));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Inbox")]
        public async Task CreateDeleteInboxAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminInbox newInbox = new NewAdminInbox("BIGTEST");
            Console.WriteLine(JsonConvert.SerializeObject(newInbox));
            AdminInbox inbox = await Connection.CreateInboxAsync(newInbox);
            Console.WriteLine(JsonConvert.SerializeObject(inbox));
            await Connection.DeleteInboxAsync(inbox.Id);
            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Field
        [TestMethod]
        [TestCategory("Field")]
        public async Task GetFieldsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminField> fields = await Connection.GetFieldsAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(fields));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Field")]
        public async Task CreateGetUpdateDeleteFieldAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminField field = new NewAdminField();
            field.Name = "Tester Field";
            field.Type = "character";
            field.Length = 50;
            Console.WriteLine(JsonConvert.SerializeObject(field));
            AdminField newField = await Connection.CreateFieldAsync(1, field);
            Console.WriteLine(JsonConvert.SerializeObject(newField));
            AdminField retrievedField = (await Connection.GetFieldsAsync(1, newField.Id))[0];
            Console.WriteLine(JsonConvert.SerializeObject(retrievedField));
            retrievedField.Name = "Tester Field Updated";
            AdminField updatedField = await Connection.UpdateFieldAsync(1, retrievedField);
            Console.WriteLine(JsonConvert.SerializeObject(updatedField));
            await Connection.DeleteFieldAsync(1, updatedField.Id);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Field")]
        public async Task GetTableFieldsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminTableField> fields = await Connection.GetTableFieldsAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(fields));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Field")]
        public async Task CreateGetUpdateDeleteTableFieldAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminTableField tableField = new NewAdminTableField();
            tableField.Name = "Tester Table Field";
            tableField.Fields.Add(2);
            Console.WriteLine(JsonConvert.SerializeObject(tableField));
            AdminTableField newTableField = await Connection.CreateTableFieldAsync(1, tableField);
            Console.WriteLine(JsonConvert.SerializeObject(newTableField));
            AdminTableField retreivedTableField = await Connection.GetTableFieldAsync(1, newTableField.Id);
            Console.WriteLine(JsonConvert.SerializeObject(retreivedTableField));
            retreivedTableField.Name = "Tester Table Field Updated";
            AdminTableField updatedTableField = await Connection.UpdateTableFieldAsync(1, retreivedTableField);
            Console.WriteLine(JsonConvert.SerializeObject(updatedTableField));
            await Connection.DeleteTableFieldAsync(1, updatedTableField.Id);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Field")]
        public async Task GetListsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<AdminList> fields = await Connection.GetListsAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(fields));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategoryAttribute("Field")]
        public async Task CreateGetUpdateDeleteListAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            NewAdminList list = new NewAdminList();
            list.Name = "Tester List";
            list.Values.Add("test value 1");
            Console.WriteLine(JsonConvert.SerializeObject(list));
            AdminList newList = await Connection.CreateListAsync(1, list);
            Console.WriteLine(JsonConvert.SerializeObject(newList));
            AdminList retrievedList = await Connection.GetListAsync(1, newList.Id);
            retrievedList.Name = "Tester List Updated";
            Console.WriteLine(JsonConvert.SerializeObject(retrievedList));
            AdminList updatedList = await Connection.UpdateListAsync(1, retrievedList);
            Console.WriteLine(JsonConvert.SerializeObject(updatedList));
            await Connection.DeleteListAsync(1, updatedList.Id);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Field")]
        public async Task LoadAssemblyListAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            AdminList list = await Connection.LoadAssemblyListAsync(1, await Connection.GetListAsync(1, 19));
            Console.WriteLine(JsonConvert.SerializeObject(list.Values));
            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Document
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetDocumentAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Result DocResult = await Connection.GetArchiveDocumentAsync(1, 14, 1);
            Console.WriteLine(JsonConvert.SerializeObject(DocResult));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetDocumentMetaDataAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            Console.WriteLine(JsonConvert.SerializeObject(document));
            Result result = await Connection.GetArchiveDocumentMetaDataAsync(1, 1, document);
            Console.WriteLine(JsonConvert.SerializeObject(result));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetDocumentFileAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            var fileName = await Connection.GetArchiveDocumentFileAsync(1, 1, document, "C:\\test\\");
            Console.WriteLine(fileName);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetDocumentThumbnailAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            var fileName = await Connection.GetArchiveDocumentThumbnailAsync(1, 1, document, "C:\\test\\", 1000, 1000);
            Console.WriteLine(fileName);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task UpdateDocumentIndexDataAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            Console.WriteLine(JsonConvert.SerializeObject(document));
            Random random = new Random();
            int num = random.Next(1000);
            document.Fields[0].Val = $"{num}";
            Console.WriteLine(JsonConvert.SerializeObject(document));
            await Connection.UpdateDocumentIndexDataAsync(1, 1, document);
            Doc updatedDoc = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            Console.WriteLine(JsonConvert.SerializeObject(updatedDoc));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task UploadIndexDeleteDocumentAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            UploadedFiles files = await Connection.UploadDocumentAsync("C:\\test\\testuploadDoc.pdf");
            Console.WriteLine(JsonConvert.SerializeObject(files));
            NewFile newFile = new NewFile();
            newFile.Files = files.Files;
            newFile.Fields.Add(new FileField("2", "Test Last Name 1234"));
            newFile.Fields.Add(new FileField("3", "11/16/2021"));
            Console.WriteLine(JsonConvert.SerializeObject(newFile));
            await Connection.ImportArchiveDocumentAsync(1, 1, newFile);
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            await Connection.DeleteArchiveDocumentAsync(1, 1, document);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task ExportDocumentsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            List<FileExport> files = new List<FileExport>();
            Console.WriteLine(JsonConvert.SerializeObject(await Connection.GetSearchResultsAsync(1, search)));
            files.Add(new FileExport(1, (await Connection.GetSearchResultsAsync(1, search)).Docs[0]));
            files.Add(new FileExport(1, (await Connection.GetSearchResultsAsync(1, search)).Docs[1]));
            Console.WriteLine(JsonConvert.SerializeObject(files));
            string exportedFiles = await Connection.ExportDocumentAsync(1, 2, files, "C:\\test\\");
            Console.WriteLine(exportedFiles);
            await Connection.DeleteLicenseAsync();
            //throw new Exception("This method has currently not been tested fully (have observed empty zip files being returned)");
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task TransferDocumentAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            int docID = await Connection.TransferArchiveDocumentAsync(1, 1, 2, document);
            Console.WriteLine(docID);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetDocumentRevisionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 1082))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            Revision revision = (await Connection.GetDocumentRevisionsAsync(1, 52, document))[0];
            Console.WriteLine(JsonConvert.SerializeObject(revision));
            Doc revisionDoc = revision.ReturnDoc();
            Console.WriteLine(JsonConvert.SerializeObject(revisionDoc));
            //await Connection.GetDocumentMetaDataAsync(1, 15, revisionDoc);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetTriggerDocumentQueueAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Search search = (await Connection.GetSearchesAsync(1, searchId: 1083))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            Console.WriteLine(JsonConvert.SerializeObject(document));
            Queue documentQueue = await Connection.GetDocumentQueueAsync(1, 53, document);
            Console.WriteLine(JsonConvert.SerializeObject(documentQueue));
            await Connection.FireDocumentQueueActionAsync(1, 53, document, documentQueue.Actions[1]);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task UploadImportIndexInboxDocumentAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            UploadedFiles files = await Connection.UploadDocumentAsync("C:\\test\\testuploadDoc.pdf");
            await Connection.ImportInboxDocumentAsync(1, files.Files[0]);

            Inbox inbox = await Connection.GetInboxAsync(1);
            Square9APIHelperLibrary.DataTypes.File file = inbox.Files[0];
            Console.WriteLine(JsonConvert.SerializeObject(file));

            List<FileField> fields = new List<FileField>();
            fields.Add(new FileField("2", "Inbox Index Test"));

            await Connection.IndexInboxDocumentAsync(1, 1, 1, file, fields);

            Search search = (await Connection.GetSearchesAsync(1, searchId: 20))[0];
            Doc document = (await Connection.GetSearchResultsAsync(1, search)).Docs[0];
            await Connection.DeleteArchiveDocumentAsync(1, 1, document);

            await Connection.DeleteInboxDocumentAsync(1, file);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task GetInboxDocumentFileAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            Inbox inbox = await Connection.GetInboxAsync(1);
            Square9APIHelperLibrary.DataTypes.File file = inbox.Files[0];
            Console.WriteLine(JsonConvert.SerializeObject(file));

            Console.WriteLine(await Connection.GetInboxDocumentFileAsync(1, file, "C:\\test\\inbox\\"));

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Document")]
        public async Task TransferInboxDocumentAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            Inbox inbox = await Connection.GetInboxAsync(1);
            Square9APIHelperLibrary.DataTypes.File file = inbox.Files[0];
            Console.WriteLine(JsonConvert.SerializeObject(file));

            Console.WriteLine(await Connection.TransferInboxDocumentAsync(1, 2, file));

            await Connection.DeleteLicenseAsync();
        }
        #endregion

        #region Administration
        [TestMethod]
        [TestCategory("Administration")]
        public async Task GetSecuredAndUnsecuredUsersAndGroupsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<SecuredGroup> securedGroups = await Connection.GetSecuredUsersAndGroupsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(securedGroups));
            List<UnsecuredGroup> unsecuredGroups = await Connection.GetUnsecuredUsersAndGroupsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(unsecuredGroups));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task GetTreeStructureAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<SecurityNode> securityNodes = await Connection.GetTreeStructureAsync();
            Console.WriteLine(JsonConvert.SerializeObject(securityNodes));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task GetUserArchivePermissionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Console.WriteLine((await Connection.GetUserArchivePermissionsAsync(2, 1, "test")).Level);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task GetUserInboxPermissionsAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            Console.WriteLine((await Connection.GetUserInboxPermissionsAsync(1, "test")).Level);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task GetUserSearchPropertiesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();
            List<SearchProperties> userSearchSecurity = await Connection.GetUserSearchPropertiesAsync(1, "SSAdministrator");
            Console.WriteLine(JsonConvert.SerializeObject(userSearchSecurity));
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task SetArchiveSecurityAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct archiveSecurity object
            ArchiveSecurity archiveSecurity = new ArchiveSecurity();

            //Add Users
            SecuredGroup securedGroup = (await Connection.GetSecuredUsersAndGroupsAsync()).Find(x => x.Name == "test");
            User user = new User();
            user.ConvertSecuredGroup(securedGroup);
            archiveSecurity.Users.Add(user);

            //Add Targets
            Target target = new Target();
            target.Id = 1; //archive ID
            target.Database = 2; //database ID
            archiveSecurity.Targets.Add(target);

            //Add Permissions
            ArchivePermission permission = new ArchivePermission((await Connection.GetUserArchivePermissionsAsync(2, 1, "test")).Level);
            permission.View = true;
            permission.Add = true;
            permission.Delete = true;
            permission.Print = true;
            permission.FullAPIAccess = true;
            permission.CalculatePermissionLevel();
            archiveSecurity.Permissions = permission;

            Console.WriteLine(JsonConvert.SerializeObject(archiveSecurity));
            await Connection.SetArchiveSecurityAsync(archiveSecurity);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task SetInboxSecurityAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct inboxSecurity object
            InboxSecurity inboxSecurity = new InboxSecurity();

            //Add Users
            SecuredGroup securedGroup = (await Connection.GetSecuredUsersAndGroupsAsync()).Find(x => x.Name == "test");
            User user = new User();
            user.ConvertSecuredGroup(securedGroup);
            inboxSecurity.Users.Add(user);

            //Add Targets
            Target target = new Target();
            target.Id = 1; //inbox ID
            inboxSecurity.Targets.Add(target);

            //Add Permissions
            InboxPermission permission = new InboxPermission();
            permission.View = true;
            permission.Add = true;
            permission.Delete = true;
            permission.Print = true;
            //permission.SelectAll();
            permission.CalculatePermissionLevel();
            inboxSecurity.Permissions = permission;

            Console.WriteLine(JsonConvert.SerializeObject(inboxSecurity));
            await Connection.SetInboxSecurityAsync(inboxSecurity);
            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task SetDatabaseSecurityAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct Database Security Object
            DatabaseSecurity databaseSecurity = new DatabaseSecurity();

            //Add Users
            SecuredGroup securedGroup = (await Connection.GetSecuredUsersAndGroupsAsync()).Find(x => x.Name == "test");
            User user = new User();
            user.ConvertSecuredGroup(securedGroup);
            databaseSecurity.Users.Add(user);

            //Add Target
            Target databaseTarget = new Target();
            databaseTarget.Id = 1;
            databaseSecurity.Targets.Add(databaseTarget);

            //Add Permissions
            DatabasePermission databasePermission = new DatabasePermission();
            databasePermission.Type = 0;
            databasePermission.License = 1;
            databaseSecurity.Permissions = databasePermission;

            await Connection.SetDatabaseSecurityAsync(databaseSecurity);

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task SetSearchSecurityAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct Search Security object
            SearchSecurity searchSecurity = new SearchSecurity();

            //Add Users
            SecuredGroup securedGroup = (await Connection.GetSecuredUsersAndGroupsAsync()).Find(x => x.Name == "test");
            User user = new User();
            user.ConvertSecuredGroup(securedGroup);
            searchSecurity.Users.Add(user);

            //Add Target
            Target searchSecurityTarget = new Target();
            searchSecurityTarget.Id = 1;
            searchSecurityTarget.Database = 2;
            searchSecurity.Targets.Add(searchSecurityTarget);

            //Add Permissions 
            SearchPermission searchPermission = new SearchPermission();
            searchPermission.View = true;
            searchSecurity.Permissions = searchPermission;

            await Connection.SetSearchSecurityAsync(searchSecurity);

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task SetSearchPropertiesAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct Search Security object
            SearchSecurity searchSecurity = new SearchSecurity();

            //Add Users
            SecuredGroup securedGroup = (await Connection.GetSecuredUsersAndGroupsAsync()).Find(x => x.Name == "test");
            User user = new User();
            user.ConvertSecuredGroup(securedGroup);
            searchSecurity.Users.Add(user);

            //Add Target
            Target searchSecurityTarget = new Target();
            searchSecurityTarget.Id = 1;
            searchSecurityTarget.Database = 2;
            searchSecurity.Targets.Add(searchSecurityTarget);

            //Add Permissions 
            SearchPermission searchPermission = new SearchPermission();
            searchPermission.Type = 8; //4=QueueSearch, 8=DefaultSearch, 16=DirectSearch
            searchSecurity.Permissions = searchPermission;

            await Connection.SetSearchPropertiesAsync(searchSecurity);

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task CreateUpdateDeleteUserAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct User Object
            User newUser = new User();
            newUser.Name = "NewTestUser";
            newUser.Password = Password;

            //Create new user
            await Connection.CreateUserAsync(newUser);

            //Get the new user
            UnsecuredGroup unsecuredGroup = (await Connection.GetUnsecuredUsersAndGroupsAsync()).Find(x => x.Name == "NewTestUser");
            User user = new User();
            user.ConvertUnsecuredGroup(unsecuredGroup);

            Console.WriteLine(JsonConvert.SerializeObject(user));

            //Update new user
            user.Password = "NewPassword123!@#";
            await Connection.UpdateUserAsync(user);

            Console.WriteLine(JsonConvert.SerializeObject(user));

            //Delete the user
            await Connection.DeleteUserAsync(user);

            await Connection.DeleteLicenseAsync();
        }
        [TestMethod]
        [TestCategory("Administration")]
        public async Task CreateGetUpdateDeleteGroupAsync()
        {
            Square9API Connection = new Square9API(Endpoint, Username, Password);
            await Connection.CreateLicenseAsync();

            //Construct Group Object
            Group newGroup = new Group();
            newGroup.Name = "NewTestGroup";

            //Create new group
            await Connection.CreateGroupAsync(newGroup);

            //Get the new group
            Group group = (await Connection.GetGroupsAsync()).Find(x => x.Name == "NewTestGroup");

            //Get some users
            List<UnsecuredGroup> unsecuredGroups = (await Connection.GetUnsecuredUsersAndGroupsAsync()).FindAll(x => x.Type == 2); //Type 2 is for S9 users

            Console.WriteLine(JsonConvert.SerializeObject(group));
            Console.WriteLine(JsonConvert.SerializeObject(unsecuredGroups));

            //Add users to new group
            group.Users.Add(unsecuredGroups[0].Name);
            group.Users.Add(unsecuredGroups[2].Name);
            group.Users.Add(unsecuredGroups[4].Name);
            group.Users.Add(unsecuredGroups[6].Name);

            //Update group
            await Connection.UpdateGroupAsync(group);

            //Get the updated group
            Group updatedGroup = (await Connection.GetGroupsAsync()).Find(x => x.Name == "NewTestGroup");

            Console.WriteLine(JsonConvert.SerializeObject(updatedGroup));

            //Delete group
            await Connection.DeleteGroupAsync(updatedGroup);

            await Connection.DeleteLicenseAsync();
        }
        #endregion
    }
}
