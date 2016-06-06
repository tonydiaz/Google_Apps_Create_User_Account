using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Services;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;


namespace NewUser_Console
{
    class Program
    {
        private const string SERVICE_ACCOUNT_ID ="<SERVICE_ACCOUNT_ID>";
        private const string SERVICE_ACCOUNT_EMAIL = "<SERVICE_ACCOUNT_EMAIL>";
        private const string SERVICE_ACCOUNT_PKCS12_FILE_PATH = @"D:\Projects\Sandbox\NewUser_Console\NewUser_Console\TestFANUserCreation-5883f2e94545.p12";

        private const string CLIENT_ID = "<SERVICE_ACCOUNT_EMAIL>";
        private const string CLIENT_SECRET = "<CLIENT_SECRET>";

        private const string USER_EMAIL = "diazam@dev.fan.gov"; //Service Account needs to be assocaited with an admin email. 

        private const string EMAIL_DOMAIN = "dev.fan.gov";

        static void Main(string[] args)
        {
            Console.WriteLine("Insert New User API Sample - Serivce Account or Client ID");
            Console.WriteLine("=========================================================");
            try
            {
                int choice = 0;
                DirectoryService service = new DirectoryService();

                User newuserbody = new User();
                UserName newusername = new UserName();
                newuserbody.PrimaryEmail = "sample@dev.fan.gov";
                newuserbody.Emails = new UserEmail()
                {
                    Address = "second_test@usg.gov",
                    Type = "custom",
                    CustomType = "Secondary Email"
                };
                //Need to be associated with a group?
                newuserbody.ChangePasswordAtNextLogin = true;
                newusername.GivenName = "John";
                newusername.FamilyName = "Doe";
                newuserbody.Name = newusername;
                newuserbody.Password = "Password01";

                Console.WriteLine("Enter 1 to create a user with the Serivce Account or 2 for the Client ID");
                choice = Int32.Parse(Console.ReadLine());

                if (choice == 1)
                {
                    //Calling the service through the Service Account
                    service = BuildServiceAccountService();
                }
                else if (choice == 2)
                {
                    //Calling the service through the Client ID
                    service = BuildClientIDService();
                }
                else
                {
                    service = BuildServiceAccountService();
                }
                
                //Search current email addresses
                var checkcurrentusers = service.Users.List();
                checkcurrentusers.Domain = EMAIL_DOMAIN;
                checkcurrentusers.Query = "email='" + newuserbody.PrimaryEmail + "'";
                var emailAvailable = checkcurrentusers.Execute();

                //Check if the User email already exists, if so skip the insert and send error
                if (emailAvailable.UsersValue == null)
                {
                    //Change the line below to either sacclient (for ServiceAccount) or clientservice (for Client ID)
                    var postinsert = service.Users.Insert(newuserbody);
                    var result = postinsert.Execute();
                }
                else
                {
                    Console.WriteLine("User email already exists");
                }
           
            }
            //Handle when a user already exists or another exception. 
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static DirectoryService BuildClientIDService()
        {
            
            //Using the Client ID for web application
            UserCredential clientCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = CLIENT_ID,
                ClientSecret = CLIENT_SECRET,
            },
            new[] { DirectoryService.Scope.AdminDirectoryUser },
            "user",
            CancellationToken.None).Result;

            //===============================================
            // Create the service using Client ID credentials.
            return new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = clientCredential,
                ApplicationName = "FAN - Create user with Client ID",
            });

        }

        private static DirectoryService BuildServiceAccountService()
        {
            //=============================
            //Using the Service account
            var certificate = new X509Certificate2(SERVICE_ACCOUNT_PKCS12_FILE_PATH, "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential serviceAccount = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(SERVICE_ACCOUNT_EMAIL)
               {
                   Scopes = new[]
                       {
                           DirectoryService.Scope.AdminDirectoryUser 
                       },
                   User = USER_EMAIL
               }.FromCertificate(certificate));

            //===============================================
            // Create the service using Service Account credentials.
            return new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = serviceAccount,
                ApplicationName = "FAN - Create User with Service account",
            });

        }

    }
}
