using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public String Subject { get; set; }

        public String Content { get; set; }
        public Message(IEnumerable<string> to,string subject,string content) {
            To=new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(string.Empty, x)));
            Subject = subject;
            Content=content;

        
        }
    }
}
