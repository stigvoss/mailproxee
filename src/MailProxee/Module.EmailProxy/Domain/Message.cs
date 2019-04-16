using MailKit;
using MimeKit;
using System.Collections.Generic;
using System.Linq;

namespace Module.EmailProxy.Infrastructure
{
    public class Message
    {
        private readonly MimeMessage _message;
        private readonly IMessageSummary _summary;

        public Message(MimeMessage message, IMessageSummary summary)
        {
            _message = message;
            _summary = summary;
        }

        public Message(string recipient, string sender, string subject, string message)
        {
            _message = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart
                {
                    Text = message
                }
            };

            _message.From.Add(new MailboxAddress(sender));
            _message.To.Add(new MailboxAddress(recipient));
        }

        public IEnumerable<string> Recipients
        {
            get
            {
                return _message.To.Mailboxes.Select(e => e.Address);
            }
            set
            {
                var senders = value.Select(address => new MailboxAddress(address));
                _message.To.Clear();

                _message.To.AddRange(senders);
            }
        }

        public IEnumerable<string> Senders
        {
            get
            {
                return _message.From.Mailboxes.Select(e => e.Address);
            }
            set
            {
                var senders = value.Select(address => new MailboxAddress(address));
                _message.From.Clear();

                _message.From.AddRange(senders);
            }
        }

        public string Subject
        {
            get
            {
                return _message.Subject;
            }
            set
            {
                _message.Subject = value;
            }
        }

        public UniqueId? UniqueId
            => _summary?.UniqueId;

        public static explicit operator MimeMessage(Message message)
            => message._message;
    }
}