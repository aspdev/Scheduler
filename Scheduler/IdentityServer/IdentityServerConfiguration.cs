namespace IdentityServer
 {
     public class IdentityServerConfiguration
     {
         public Certificates Certificates { get; set; }
         public MailService MailService { get; set; }
         public string Database { get; set; }
         public string DatabaseUrl { get; set; }
     }
 
     public class Certificates
     {
         public string TokenCertificatePassword { get; set; }
         public string RavenCertificatePassword { get; set; }
         public string RavenCertificatePath { get; set; }
     }
 
     public class MailService
     {
         public string MailBoxPassword { get; set; }
         public string Host { get; set; }
         public int Port { get; set; }
         public bool UseSsl { get; set; }
         public string MailBoxAddress { get; set; }
         public string FromName { get; set; }
     }
 }