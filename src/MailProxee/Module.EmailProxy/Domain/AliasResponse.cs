using MimeKit;

namespace Module.EmailProxy.Domain
{
    public class AliasResponse
    {
        public string Subject { get; internal set; }

        public MailboxAddress Recipient { get; internal set; }

        public MailboxAddress Sender { get; internal set; }

        public string Content { get; internal set; }

        public static implicit operator MimeMessage(AliasResponse response)
        {
            var message = new MimeMessage
            {
                Subject = response.Subject,
                Body = new TextPart("plain")
                {
                    Text = response.Content
                }
            };

            message.From.Add(response.Sender);
            message.To.Add(response.Recipient);

            return message;
        }
    }
}