﻿using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Text;
using MediatorLib;

namespace MediatorBot
{
    [Serializable]
    public class MediatorDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(ProcessMessage);
        }

        private async Task ProcessMessage(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var msg = await result;
            if (msg.Text != "!users" && msg.Text != "!stats")
            {
                ConversationState.RegisterMessage(msg.From.Name, msg.Text);
            }

            var badSentimenCheck = ConversationState.Users.Where((u => u.Sentiment < 0.4 && u.Sentiment != 0));
            if (badSentimenCheck.Any())
            {
                ConversationState.TextAnalysisDocumentStore phrasesDoc = await ConversationState.GetPhrasesforConversation();

                await context.PostAsync(BuildReply(
                sb =>
                {
                    foreach (string x in phrasesDoc.documents[0].keyPhrases)
                    {
                        sb.AppendLine($"Phrase: {x}"); }
                     }));
            }

            if (msg.Text == "!users")
            {
                await context.PostAsync(BuildReply(
                    sb =>
                    {
                        ConversationState.Users.ForEach(x => sb.AppendLine(x.name));
                    }));
            }
            else if (msg.Text == "!stats")
            {
                await context.PostAsync(BuildReply(
                    sb =>
                    {
                        foreach (var x in ConversationState.Users)
                        { sb.AppendLine($"{x.name}: msgs={x.MessageCount}, sentiment={x.Sentiment}"); }
                    }));
            }
            else
            {
                await context.PostAsync("");
            }
            context.Wait(ProcessMessage);
        }

        protected string BuildReply(Action<StringBuilder> body)
        {
            var sb = new StringBuilder();
            body(sb);
            return sb.ToString();
        }
    }
}