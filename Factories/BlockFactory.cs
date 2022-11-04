using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Bot.Builder.Community.Adapters.Slack.Model;
using System.Threading.Tasks;
using SlackAPI;

namespace Ada.Helpers
{
    public static class BlockFactory
    {
        public static IActivity AsActivity(this IList<IBlock> blocks) =>
            MessageFactory.Attachment(
                new Microsoft.Bot.Schema.Attachment
                {
                    Content = blocks.ToArray(),
                    ContentType = "application/json",
                    Name = "blocks",
                });

        public static IList<IBlock> AddContextBlock(this IList<IBlock> blocks, string text)
        {
            blocks.Add(
                (ContextBlock)new()
                {
                    elements = new[]
                    {
                    new Text
                    {
                        type = TextTypes.Markdown,
                        text = text
                    }
                    }
                });

            return blocks;
        }

        public static IList<IBlock> AddDividerBlock(this IList<IBlock> blocks)
        {
            blocks.Add(new DividerBlock());
            return blocks;
        }

        public static IList<IBlock> AddMarkdownBlock(this IList<IBlock> blocks, string markdown)
        {
            blocks.Add(
              (SectionBlock)new()
              {
                  text = new Text
                  {
                      type = TextTypes.Markdown,
                      text = markdown
                  },
              });

            return blocks;
        }

        public static IList<IBlock> CreateNew => new List<IBlock>();
    }
}