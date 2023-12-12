using MassTransit;
using UploadFiles.Shared.Contracts;

namespace RankText
{
    public class RankTextConsumer : IConsumer<RankTextMessage>
    {
        public RankTextConsumer()
        {
        }

        public async Task Consume(ConsumeContext<RankTextMessage> context)
        {
           
        }

   
    }
}
