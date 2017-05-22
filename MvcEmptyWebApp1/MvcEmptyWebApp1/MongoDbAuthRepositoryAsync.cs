using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ServiceStack.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ServiceStack.Authentication.MongoDb
{
    public class MongoDbAuthRepositoryAsync : IUserAuthRepository, IClearable, IManageApiKeys
    {
        private readonly IMongoDatabase mongoDatabase;

        private static string CountersName
        {
            get
            {
                return typeof(Counters).Name;
            }
        }

        private static string UserAuthName
        {
            get
            {
                return typeof(UserAuth).Name;
            }
        }

        private static string UserOAuthProviderName
        {
            get
            {
                return typeof(UserAuthDetails).Name;
            }
        }

        public MongoDbAuthRepositoryAsync(IMongoDatabase mongoDatabase, bool createMissingCollections)
        {
            this.mongoDatabase = mongoDatabase;
            if (createMissingCollections)
            {
                CreateMissingCollections();
            }
            if (!CollectionsExists())
            {
                throw new InvalidOperationException("One of the collections needed by MongoDbAuth2Repository is missing.You can call MongoDbAuth2Repository constructor with the parameter CreateMissingCollections set to 'true'  to create the needed collections.");
            }
        }

        private static void AssertNoExistingUser(IMongoDatabase mongoDatabase, IUserAuth newUser, IUserAuth exceptForExistingUser = null)
        {
            if (newUser.UserName != null)
            {
                UserAuth userAuthByUserName = GetUserAuthByUserName(mongoDatabase, newUser.UserName);
                if (userAuthByUserName != null && (exceptForExistingUser == null || userAuthByUserName.Id != exceptForExistingUser.Id))
                {
                    object[] userName = new object[] { newUser.UserName };
                    throw new ArgumentException("User {0} already exists".Fmt(userName));
                }
            }
            if (newUser.Email != null)
            {
                UserAuth userAuth = GetUserAuthByUserName(mongoDatabase, newUser.Email);
                if (userAuth != null && (exceptForExistingUser == null || userAuth.Id != exceptForExistingUser.Id))
                {
                    object[] email = new object[] { newUser.Email };
                    throw new ArgumentException("Email {0} already exists".Fmt(email));
                }
            }
        }

        private bool CollectionExist(string collectionName)
        {
            var collection = mongoDatabase.ListCollections(new ListCollectionsOptions { Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName) });
            if (collection.MoveNext())
            {
                return true;
            }
            return false;
        }


        public void Clear()
        {
            DropAndReCreateCollections();
        }

        public bool CollectionsExists()
        {
            if (!CollectionExist(UserAuthName) || !CollectionExist(UserOAuthProviderName))
            {
                return false;
            }
            return CollectionExist(CountersName);
        }

        public void CreateMissingCollections()
        {
            if (!CollectionExist(UserAuthName))
            {
                mongoDatabase.CreateCollection(UserAuthName);
            }
            if (!CollectionExist(UserOAuthProviderName))
            {
                mongoDatabase.CreateCollection(UserOAuthProviderName);
            }
            if (!CollectionExist(CountersName))
            {
                mongoDatabase.CreateCollection(CountersName);
                var collection = mongoDatabase.GetCollection<Counters>(CountersName);
                collection.InsertOne(new Counters());
            }
        }

        public IUserAuthDetails CreateOrMergeAuthSession(IAuthSession authSession, IAuthTokens tokens)
        {
            object userAuth = this.GetUserAuth(authSession, tokens) ?? new UserAuth();
            var utcNow = (IUserAuth)userAuth;

            var providerFilter = Builders<UserAuthDetails>.Filter.Eq("Provider", tokens.Provider);
            var userIdFilter = Builders<UserAuthDetails>.Filter.Eq("ClientId", tokens.UserId);

            var collection = mongoDatabase.GetCollection<UserAuthDetails>(UserOAuthProviderName);
            UserAuthDetails id = collection.Find(Builders<UserAuthDetails>.Filter.And(new[] { providerFilter, userIdFilter })).FirstOrDefault();
            if (id == null)
            {
                UserAuthDetails userAuthDetail = new UserAuthDetails()
                {
                    Provider = tokens.Provider,
                    UserId = tokens.UserId
                };
                id = userAuthDetail;
            }
            id.PopulateMissing(tokens, false);
            utcNow.PopulateMissingExtended(id, false);
            utcNow.ModifiedDate = DateTime.UtcNow;
            if (utcNow.CreatedDate == new DateTime())
            {
                utcNow.CreatedDate = utcNow.ModifiedDate;
            }
            this.SaveUser((UserAuth)utcNow);
            if (id.Id == 0)
            {
                id.Id = this.IncUserOAuthProviderCounter();
            }
            id.UserAuthId = utcNow.Id;
            if (id.CreatedDate == new DateTime())
            {
                id.CreatedDate = utcNow.ModifiedDate;
            }
            id.ModifiedDate = utcNow.ModifiedDate;
            collection.ReplaceOne(x => x.Id == id.Id, id, new UpdateOptions() { IsUpsert = true });
            return id;
        }


        public IUserAuth CreateUserAuth(IUserAuth newUser, string password)
        {
            string str;
            string str1;
            this.ValidateNewUser(newUser, password);
            AssertNoExistingUser(this.mongoDatabase, newUser, null);
            HostContext.Resolve<IHashProvider>().GetHashAndSaltString(password, out str1, out str);
            DigestAuthFunctions digestAuthFunction = new DigestAuthFunctions();
            newUser.DigestHa1Hash = digestAuthFunction.CreateHa1(newUser.UserName, DigestAuthProvider.Realm, password);
            newUser.PasswordHash = str1;
            newUser.Salt = str;
            newUser.CreatedDate = DateTime.UtcNow;
            newUser.ModifiedDate = newUser.CreatedDate;
            this.SaveUser(newUser);
            return newUser;
        }

        public ServiceStack.Auth.IUserAuth UpdateUserAuth(ServiceStack.Auth.IUserAuth existingUser, ServiceStack.Auth.IUserAuth newUser)
        {
            throw new NotImplementedException();
        }

        public void DeleteUserAuth(string userAuthId)
        {
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuthName);
            collection.DeleteOne(Builders<UserAuth>.Filter.Eq("_id", int.Parse(userAuthId)));

            var mongoCollection = mongoDatabase.GetCollection<UserAuthDetails>(UserOAuthProviderName);
            mongoCollection.DeleteOne(Builders<UserAuthDetails>.Filter.Eq("UserAuthId", int.Parse(userAuthId)));
        }


        public void DropAndReCreateCollections()
        {
            if (CollectionExist(UserAuthName))
            {
                mongoDatabase.DropCollection(UserAuthName);
            }
            if (CollectionExist(UserOAuthProviderName))
            {
                mongoDatabase.DropCollection(UserOAuthProviderName);
            }
            if (CollectionExist(CountersName))
            {
                mongoDatabase.DropCollection(CountersName);
            }
            CreateMissingCollections();
        }

        public IUserAuth GetUserAuth(string userAuthId)
        {
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuthName);
            return collection.Find(x => x.Id == int.Parse(userAuthId)).FirstOrDefault();
        }


        public IUserAuth GetUserAuth(IAuthSession authSession, IAuthTokens tokens)
        {
            if (!authSession.UserAuthId.IsNullOrEmpty())
            {
                IUserAuth userAuth = this.GetUserAuth(authSession.UserAuthId);
                if (userAuth != null)
                {
                    return userAuth;
                }
            }
            if (!authSession.UserAuthName.IsNullOrEmpty())
            {
                IUserAuth userAuthByUserName = this.GetUserAuthByUserName(authSession.UserAuthName);
                if (userAuthByUserName != null)
                {
                    return userAuthByUserName;
                }
            }
            if (tokens == null || tokens.Provider.IsNullOrEmpty() || tokens.UserId.IsNullOrEmpty())
            {
                return null;
            }
            var collection = mongoDatabase.GetCollection<UserAuthDetails>(UserOAuthProviderName);
            UserAuthDetails userAuthDetail =
                collection.Find(x => x.Provider == tokens.Provider && x.UserId == tokens.UserId)
                        .FirstOrDefault();

            if (userAuthDetail == null)
            {
                return null;
            }
            var mongoCollection = this.mongoDatabase.GetCollection<UserAuth>(UserAuthName);


            return mongoCollection.Find(Builders<UserAuth>.Filter.Eq("_id", userAuthDetail.UserAuthId)).FirstOrDefault();
        }


        public IUserAuth GetUserAuthByUserName(string userNameOrEmail)
        {
            return GetUserAuthByUserName(mongoDatabase, userNameOrEmail);
        }

        private static UserAuth GetUserAuthByUserName(IMongoDatabase mongoDatabase, string userNameOrEmail)
        {
            if (userNameOrEmail == null)
            {
                return null;
            }
            bool flag = userNameOrEmail.Contains("@");
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuthName);

            if (flag)
            {
                try
                {
                    return collection.Find(x => x.Email == userNameOrEmail).FirstOrDefault();
                }
                catch (Exception e)
                {
                    var bam = e;
                    return null;
                }
            }

            return collection.Find(x => x.UserName == userNameOrEmail).FirstOrDefault();
        }

        public List<IUserAuthDetails> GetUserAuthDetails(string userAuthId)
        {
            var collection = mongoDatabase.GetCollection<UserAuthDetails>(UserOAuthProviderName);
            return collection.Find(x => x.UserAuthId == int.Parse(userAuthId)).ToList().ConvertAll<IUserAuthDetails>(x => x);

        }

        private Counters IncCounter(string counterName)
        {
            var collection = this.mongoDatabase.GetCollection<Counters>(CountersName);
            /*FindAndModifyArgs findAndModifyArg = new FindAndModifyArgs();
            findAndModifyArg.set_Query(Query.get_Null());
            findAndModifyArg.set_SortBy(SortBy.get_Null());
            findAndModifyArg.set_Update(Update.Inc(counterName, 1));
            findAndModifyArg.set_Upsert(true);
            return collection.FindAndModify(findAndModifyArg).GetModifiedDocumentAs<Counters>();*/

            var update = Builders<Counters>.Update.Inc(counterName, 1);
            collection.UpdateOne(_ => true, update, new UpdateOptions { IsUpsert = true });
            return collection.Find(_ => true).FirstOrDefault();
        }

        private int IncUserAuthCounter()
        {
            return  this.IncCounter("UserAuthCounter").UserAuthCounter;
        }

        private int IncUserOAuthProviderCounter()
        {
            return this.IncCounter("UserOAuthProviderCounter").UserOAuthProviderCounter;
        }

        public void LoadUserAuth(IAuthSession session, IAuthTokens tokens)
        {
            session.ThrowIfNull("session");
            this.LoadUserAuth(session, this.GetUserAuth(session, tokens));
        }

        private void LoadUserAuth(IAuthSession session, IUserAuth userAuth)
        {
            session.PopulateSession(userAuth, this.GetUserAuthDetails(session.UserAuthId).ConvertAll<IAuthTokens>((IUserAuthDetails x) => x));
        }
        private void SaveUser(IUserAuth userAuth)
        {
            if (userAuth.Id == 0)
            {
                userAuth.Id = this.IncUserAuthCounter();
            }

            var collection = this.mongoDatabase.GetCollection<UserAuth>(UserAuthName);


            collection.ReplaceOne<UserAuth>(x => x.Id == userAuth.Id, (UserAuth)userAuth, new UpdateOptions() { IsUpsert = true });
        }

        public void SaveUserAuth(IAuthSession authSession)
        {
            UserAuth utcNow = (!authSession.UserAuthId.IsNullOrEmpty() ? (UserAuth)this.GetUserAuth(authSession.UserAuthId) : authSession.ConvertTo<UserAuth>());
            if (utcNow.Id == 0 && !authSession.UserAuthId.IsNullOrEmpty())
            {
                utcNow.Id = int.Parse(authSession.UserAuthId);
            }
            utcNow.ModifiedDate = DateTime.UtcNow;
            if (utcNow.CreatedDate == new DateTime())
            {
                utcNow.CreatedDate = utcNow.ModifiedDate;
            }
            this.mongoDatabase.GetCollection<UserAuth>(UserAuthName);
            this.SaveUser(utcNow);
        }

        public void SaveUserAuth(IUserAuth userAuth)
        {
            userAuth.ModifiedDate = DateTime.UtcNow;
            if (userAuth.CreatedDate == new DateTime())
            {
                userAuth.CreatedDate = userAuth.ModifiedDate;
            }
            this.SaveUser(userAuth);
        }

        public bool TryAuthenticate(string userName, string password, out IUserAuth userAuth)
        {
            userAuth = this.GetUserAuthByUserName(userName);
            if (userAuth == null)
            {
                return false;
            }
            if (HostContext.Resolve<IHashProvider>().VerifyHashString(password, userAuth.PasswordHash, userAuth.Salt))
            {
                return true;
            }
            userAuth = null;
            return false;
        }

        public bool TryAuthenticate(Dictionary<string, string> digestHeaders, string PrivateKey, int NonceTimeOut, string sequence, out IUserAuth userAuth)
        {
            userAuth = this.GetUserAuthByUserName(digestHeaders["username"]);
            if (userAuth == null)
            {
                return false;
            }
            if ((new DigestAuthFunctions()).ValidateResponse(digestHeaders, PrivateKey, NonceTimeOut, userAuth.DigestHa1Hash, sequence))
            {
                return true;
            }
            userAuth = null;
            return false;
        }

        public IUserAuth UpdateUserAuth(IUserAuth existingUser, IUserAuth newUser, string password)
        {
            this.ValidateNewUser(newUser, password);
            AssertNoExistingUser(this.mongoDatabase, newUser, existingUser);
            string passwordHash = existingUser.PasswordHash;
            string salt = existingUser.Salt;
            if (password != null)
            {
                HostContext.Resolve<IHashProvider>().GetHashAndSaltString(password, out passwordHash, out salt);
            }
            string digestHa1Hash = existingUser.DigestHa1Hash;
            if (password != null || existingUser.UserName != newUser.UserName)
            {
                DigestAuthFunctions digestAuthFunction = new DigestAuthFunctions();
                digestHa1Hash = digestAuthFunction.CreateHa1(newUser.UserName, DigestAuthProvider.Realm, password);
            }
            newUser.Id = existingUser.Id;
            newUser.PasswordHash = passwordHash;
            newUser.Salt = salt;
            newUser.DigestHa1Hash = digestHa1Hash;
            newUser.CreatedDate = existingUser.CreatedDate;
            newUser.ModifiedDate = DateTime.UtcNow;
            this.SaveUser(newUser);
            return newUser;
        }

        private void ValidateNewUser(IUserAuth newUser, string password)
        {
            newUser.ThrowIfNull("newUser");
            password.ThrowIfNullOrEmpty("password");
            if (newUser.UserName.IsNullOrEmpty() && newUser.Email.IsNullOrEmpty())
            {
                throw new ArgumentNullException("UserName or Email is required");
            }
            if (!newUser.UserName.IsNullOrEmpty() && !HostContext.GetPlugin<AuthFeature>().IsValidUsername(newUser.UserName))
            {
               // throw new ArgumentException("UserName contains invalid characters", "UserName");
            }
        }

        private class Counters
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id
            {
                get;
                set;
            }

            public int UserAuthCounter
            {
                get;
                set;
            }

            public int UserOAuthProviderCounter
            {
                get;
                set;
            }

            public Counters()
            {
            }
        }

        public void InitApiKeySchema()
        {
            if (!CollectionExist("ApiKey"))
            {
                mongoDatabase.CreateCollection("ApiKey");
            }
        }

        public bool ApiKeyExists(string apiKey)
        {
            /*bool byId;
            using (IRedisClientFacade client = this.factory.GetClient())
            {
                byId = client.As<ApiKey>().GetById(apiKey) != null;
            }
            return byId;*/
            var collection = mongoDatabase.GetCollection<ApiKey>("ApiKey");
            var key = collection.Find(x => x.Id == apiKey).FirstOrDefault();
            return key != null;
        }

        public ApiKey GetApiKey(string apiKey)
        {
            /*ApiKey byId;
            using (IRedisClientFacade client = this.factory.GetClient())
            {
                byId = client.As<ApiKey>().GetById(apiKey);
            }
            return byId;*/

            var collection = mongoDatabase.GetCollection<ApiKey>("ApiKey");
            return collection.Find(x => x.Id == apiKey).FirstOrDefault();
        }

        public List<ApiKey> GetUserApiKeys(string userId)
        {
            var collection = mongoDatabase.GetCollection<ApiKey>("ApiKey");
            var apiKeys = collection.Find(x => x.UserAuthId == userId).ToList();

            return apiKeys.Where((ApiKey x) =>
            {
                if (x.CancelledDate.HasValue)
                {
                    return false;
                }
                if (!x.ExpiryDate.HasValue)
                {
                    return true;
                }
                DateTime? expiryDate = x.ExpiryDate;
                DateTime utcNow = DateTime.UtcNow;
                if (!expiryDate.HasValue)
                {
                    return false;
                }
                return expiryDate.GetValueOrDefault() >= utcNow;
            }).OrderByDescending<ApiKey, DateTime>((ApiKey x) => x.CreatedDate).ToList<ApiKey>();


        }

        public void StoreAll(IEnumerable<ApiKey> apiKeys)
        {
            foreach (ApiKey apiKey in apiKeys)
            {
                long num = long.Parse(apiKey.UserAuthId);
                var collection = mongoDatabase.GetCollection<ApiKey>("ApiKey");
                collection.ReplaceOne(x => x.Id == apiKey.Id, apiKey, new UpdateOptions() { IsUpsert = true });
            }
        }
    }
}