using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;
using System.Web.Hosting;
using System.Web.Security;
using DevOne.Security.Cryptography.BCrypt;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Compilify.Web.Infrastructure.Providers
{
    public class MongoDBMembershipProvider : MembershipProvider
    {
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private int maxInvalidPasswordAttempts;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private MongoCollection mongoCollection;
        private int passwordAttemptWindow;
        private MembershipPasswordFormat passwordFormat;
        private string passwordStrengthRegularExpression;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (!VerifyPassword(bsonDocument, oldPassword))
            {
                return false;
            }

            var validatePasswordEventArgs = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(validatePasswordEventArgs);

            if (validatePasswordEventArgs.Cancel)
            {
                throw new MembershipPasswordException(validatePasswordEventArgs.FailureInformation.Message);
            }

            var update = Update.Set("LastPasswordChangedDate", DateTime.UtcNow)
                               .Set("Password", EncodePassword(newPassword, PasswordFormat, bsonDocument["Salt"].AsString));

            return mongoCollection.Update(query, update, SafeMode.True).Ok;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, 
                                                             string newPasswordQuestion, string newPasswordAnswer)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (!VerifyPassword(bsonDocument, password))
            {
                return false;
            }

            var update = Update.Set("PasswordQuestion", newPasswordQuestion)
                               .Set("PasswordAnswer", EncodePassword(newPasswordAnswer, PasswordFormat, bsonDocument["Salt"].AsString));

            return mongoCollection.Update(query, update, SafeMode.True).Ok;
        }

        public override MembershipUser CreateUser(string username, string password, string email, 
                                                  string passwordQuestion, string passwordAnswer, bool isApproved, 
                                                  object providerUserKey, out MembershipCreateStatus status)
        {
            if (providerUserKey != null)
            {
                if (!(providerUserKey is Guid))
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }
            }
            else
            {
                providerUserKey = Guid.NewGuid();
            }

            var validatePasswordEventArgs = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(validatePasswordEventArgs);

            if (validatePasswordEventArgs.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresQuestionAndAnswer && !string.IsNullOrWhiteSpace(passwordQuestion))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if (RequiresQuestionAndAnswer && !string.IsNullOrWhiteSpace(passwordAnswer))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if (GetUser(username, false) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            if (GetUser(providerUserKey, false) != null)
            {
                status = MembershipCreateStatus.DuplicateProviderUserKey;
                return null;
            }

            if (RequiresUniqueEmail && !string.IsNullOrWhiteSpace(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var salt = BCryptHelper.GenerateSalt();
            var creationDate = DateTime.UtcNow;

            var bsonDocument = new BsonDocument
            {
                { "_id", (Guid)providerUserKey },
                { "ApplicationName", ApplicationName },
                { "CreationDate", creationDate },
                { "Email", email },
                { "FailedPasswordAnswerAttemptCount", 0 },
                { "FailedPasswordAnswerAttemptWindowStart", creationDate },
                { "FailedPasswordAttemptCount", 0 },
                { "FailedPasswordAttemptWindowStart", creationDate },
                { "IsApproved", isApproved },
                { "IsLockedOut", false },
                { "LastActivityDate", creationDate },
                { "LastLockoutDate", new DateTime(1970, 1, 1) },
                { "LastLoginDate", creationDate },
                { "LastPasswordChangedDate", creationDate },
                { "Password", EncodePassword(password, PasswordFormat, salt) },
                { "PasswordAnswer", EncodePassword(passwordAnswer, PasswordFormat, salt) },
                { "PasswordQuestion", passwordQuestion },
                { "Salt", salt },
                { "Username", username }
            };

            var result = mongoCollection.Insert(bsonDocument, SafeMode.True);
            if (!result.Ok)
            {
                throw new ProviderException(result.ErrorMessage ?? "An error occurred while creating the user.");
            }

            status = MembershipCreateStatus.Success;
            return GetUser(username, false);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            return mongoCollection.Remove(query).Ok;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, 
                                                                  out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();

            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.Matches("Email", emailToMatch));
            totalRecords = (int)mongoCollection.FindAs<BsonDocument>(query).Count();

            foreach (var bsonDocument in mongoCollection.FindAs<BsonDocument>(query).SetSkip(pageIndex * pageSize).SetLimit(pageSize))
            {
                membershipUsers.Add(ToMembershipUser(bsonDocument));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, 
                                                                 out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();

            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.Matches("Username", usernameToMatch));
            totalRecords = (int)mongoCollection.FindAs<BsonDocument>(query).Count();

            foreach (var bsonDocument in mongoCollection.FindAs<BsonDocument>(query).SetSkip(pageIndex * pageSize).SetLimit(pageSize))
            {
                membershipUsers.Add(ToMembershipUser(bsonDocument));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = new MembershipUserCollection();

            var query = Query.EQ("ApplicationName", ApplicationName);
            totalRecords = (int)mongoCollection.FindAs<BsonDocument>(query).Count();

            var page = mongoCollection.FindAs<BsonDocument>(query).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
            foreach (var bsonDocument in page)
            {
                membershipUsers.Add(ToMembershipUser(bsonDocument));
            }

            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException("This Membership Provider has not been configured to support password retrieval.");
            }

            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (RequiresQuestionAndAnswer && !VerifyPasswordAnswer(bsonDocument, answer))
            {
                throw new MembershipPasswordException("The password-answer supplied is invalid.");
            }

            return DecodePassword(bsonDocument["Password"].AsString, PasswordFormat);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (bsonDocument == null)
            {
                return null;
            }

            return ToMembershipUser(bsonDocument);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var query = Query.EQ("_id", (Guid)providerUserKey);
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (bsonDocument == null)
            {
                return null;
            }

            return ToMembershipUser(bsonDocument);
        }

        public override string GetUserNameByEmail(string email)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Email", email));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);
            return bsonDocument != null ? bsonDocument["Username"].AsString : null;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            ApplicationName = config["applicationName"] ?? HostingEnvironment.ApplicationVirtualPath;
            enablePasswordReset = Boolean.Parse(config["enablePasswordReset"] ?? "true");
            enablePasswordRetrieval = Boolean.Parse(config["enablePasswordRetrieval"] ?? "false");
            maxInvalidPasswordAttempts = Int32.Parse(config["maxInvalidPasswordAttempts"] ?? "5");
            minRequiredNonAlphanumericCharacters = Int32.Parse(config["minRequiredNonAlphanumericCharacters"] ?? "1");
            minRequiredPasswordLength = Int32.Parse(config["minRequiredPasswordLength"] ?? "7");
            passwordAttemptWindow = Int32.Parse(config["passwordAttemptWindow"] ?? "10");
            passwordFormat = (MembershipPasswordFormat)Enum.Parse(typeof(MembershipPasswordFormat), config["passwordFormat"] ?? "Hashed");
            passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"] ?? string.Empty;
            requiresQuestionAndAnswer = Boolean.Parse(config["requiresQuestionAndAnswer"] ?? "false");
            requiresUniqueEmail = Boolean.Parse(config["requiresUniqueEmail"] ?? "true");

            if (PasswordFormat == MembershipPasswordFormat.Hashed && EnablePasswordRetrieval)
            {
                throw new ProviderException("Configured settings are invalid: Hashed passwords cannot be retrieved. Either set the password format to different type, or set enablePasswordRetrieval to false.");
            }

            mongoCollection = MongoServer.Create(config["connectionString"] ?? "mongodb://localhost")
                                         .GetDatabase(config["database"] ?? "ASPNETDB")
                                         .GetCollection(config["collection"] ?? "Users");

            mongoCollection.EnsureIndex("ApplicationName");
            mongoCollection.EnsureIndex("ApplicationName", "Email");
            mongoCollection.EnsureIndex("ApplicationName", "Username");

            base.Initialize(name, config);
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");
            }

            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (RequiresQuestionAndAnswer && !VerifyPasswordAnswer(bsonDocument, answer))
            {
                throw new MembershipPasswordException("The password-answer supplied is invalid.");
            }

            var password = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
            
            Update.Set("LastPasswordChangedDate", DateTime.UtcNow)
                  .Set("Password", EncodePassword(password, PasswordFormat, bsonDocument["Salt"].AsString));

            return password;
        }

        public override bool UnlockUser(string username)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var update = Update.Set("FailedPasswordAttemptCount", 0).Set("FailedPasswordAttemptWindowStart", new DateTime(1970, 1, 1)).Set("FailedPasswordAnswerAttemptCount", 0).Set("FailedPasswordAnswerAttemptWindowStart", new DateTime(1970, 1, 1)).Set("IsLockedOut", false).Set("LastLockoutDate", new DateTime(1970, 1, 1));
            return mongoCollection.Update(query, update).Ok;
        }

        public override void UpdateUser(MembershipUser user)
        {
            var query = Query.EQ("_id", (Guid)user.ProviderUserKey);
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (bsonDocument == null)
            {
                throw new ProviderException("The user was not found.");
            }

            var update = Update.Set("ApplicationName", ApplicationName)
                .Set("Comment", user.Comment)
                .Set("Email", user.Email)
                .Set("IsApproved", user.IsApproved)
                .Set("LastActivityDate", user.LastActivityDate.ToUniversalTime())
                .Set("LastLoginDate", user.LastLoginDate.ToUniversalTime());

            mongoCollection.Update(query, update);
        }

        public override bool ValidateUser(string username, string password)
        {
            var query = Query.And(Query.EQ("ApplicationName", ApplicationName), Query.EQ("Username", username));
            var bsonDocument = mongoCollection.FindOneAs<BsonDocument>(query);

            if (bsonDocument == null || !bsonDocument["IsApproved"].AsBoolean || bsonDocument["IsLockedOut"].AsBoolean)
            {
                return false;
            }

            if (VerifyPassword(bsonDocument, password))
            {
                mongoCollection.Update(query, Update.Set("LastLoginDate", DateTime.UtcNow));
                return true;
            }

            mongoCollection.Update(query, Update.Inc("FailedPasswordAttemptCount", 1).Set("FailedPasswordAttemptWindowStart", DateTime.UtcNow));
            return false;
        }

        private string DecodePassword(string password, MembershipPasswordFormat membershipPasswordFormat)
        {
            switch (membershipPasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    return password;

                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Hashed passwords cannot be decoded.");

                default:
                    var passwordBytes = Convert.FromBase64String(password);
                    var decryptedBytes = DecryptPassword(passwordBytes);
                    return decryptedBytes == null ? null : Encoding.Unicode.GetString(decryptedBytes, 16, decryptedBytes.Length - 16);
            }
        }

        private string EncodePassword(string password, MembershipPasswordFormat membershipPasswordFormat, string salt)
        {
            if (password == null)
            {
                return null;
            }

            if (membershipPasswordFormat == MembershipPasswordFormat.Clear)
            {
                return password;
            }

            if (membershipPasswordFormat == MembershipPasswordFormat.Hashed)
            {
                return BCryptHelper.HashPassword(password, salt);
            }
            
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var saltBytes = Convert.FromBase64String(salt);
            var allBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, allBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, allBytes, saltBytes.Length, passwordBytes.Length);

            return Convert.ToBase64String(EncryptPassword(allBytes));
        }

        private MembershipUser ToMembershipUser(BsonDocument bsonDocument)
        {
            if (bsonDocument == null)
            {
                return null;
            }

            var comment = bsonDocument.Contains("Comment") ? bsonDocument["Comment"].AsString : null;
            var email = bsonDocument.Contains("Email") ? bsonDocument["Email"].AsString : null;
            var passwordQuestion = bsonDocument.Contains("PasswordQuestion") ? bsonDocument["PasswordQuestion"].AsString : null;

            return new MembershipUser(Name, bsonDocument["Username"].AsString, bsonDocument["_id"].AsGuid, email, passwordQuestion, comment, bsonDocument["IsApproved"].AsBoolean, bsonDocument["IsLockedOut"].AsBoolean, bsonDocument["CreationDate"].AsDateTime, bsonDocument["LastLoginDate"].AsDateTime, bsonDocument["LastActivityDate"].AsDateTime, bsonDocument["LastPasswordChangedDate"].AsDateTime, bsonDocument["LastLockoutDate"].AsDateTime);
        }

        private bool VerifyPassword(BsonDocument user, string password)
        {
            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                return BCryptHelper.CheckPassword(password, user["Password"].AsString);
            }

            return user["Password"].AsString == EncodePassword(password, PasswordFormat, user["Salt"].AsString);
        }

        private bool VerifyPasswordAnswer(BsonDocument user, string passwordAnswer)
        {
            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                return BCryptHelper.CheckPassword(passwordAnswer, user["PasswordAnswer"].AsString);
            }

            return user["PasswordAnswer"].AsString == EncodePassword(passwordAnswer, PasswordFormat, user["Salt"].AsString);
        }
    }
}
