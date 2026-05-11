using UnityEngine;
using UnityEngine.Networking;

namespace Connect.Core {
    public static class MailHelper {
        
        public static void SendEmail() {
            var email = "mrsanjayvnair@mail.com";
            var subject = MyEscapeURL("Hello!");
            var body = MyEscapeURL("This is the message body.");
        
            // Use the Application class to open the mail client
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        private static string MyEscapeURL(string url) {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }
    }
}